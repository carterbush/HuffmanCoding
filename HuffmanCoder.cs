using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Huffman
{

    public class HuffmanCoder
    {
        public readonly TreeNode Root;
        public readonly Dictionary<char, bool[]> EncodingTable;

        private HuffmanCoder(TreeNode root, Dictionary<char, bool[]> encodingTable)
        {
            this.Root = root;
            this.EncodingTable = encodingTable;
        }

        private static HuffmanCoder CreateFromText(string text)
        {
            var leafNodes = text.ToCharArray()
                                .GroupBy(c => c)
                                .Select(g => new TreeNode(g.Key, ((double)g.Count()) / text.Length));

            return HuffmanCoder.Create(leafNodes);
        }

        private static HuffmanCoder CreateFromSignature(string signature)
        {
            var leafNodes = signature.Split("|^").Select(s =>
            {
                var ss = s.Split(",^");
                var c = Utilities.StrToChar(ss[0]);
                var probValue = double.Parse(ss[1]);

                return new TreeNode(c, probValue);
            });

            return HuffmanCoder.Create(leafNodes);
        }

        private static HuffmanCoder Create(IEnumerable<TreeNode> leafNodes)
        {
            // Ghetto priority queue to build the tree
            var queue = new SortedSet<TreeNode>(leafNodes, TreeNode.Comparer);
            while (queue.Count > 1)
            {
                var t1 = queue.Max;
                queue.Remove(t1);

                var t2 = queue.Max;
                queue.Remove(t2);

                queue.Add(new TreeNode(t1, t2));
            }
            var root = queue.Max;

            // Build the encoding table via DFS
            var encodingTable = new Dictionary<char, bool[]>();
            var stack = new Stack<(TreeNode, bool[])>();
            stack.Push((root, new bool[0]));
            while (stack.Count > 0)
            {
                (var node, var path) = stack.Pop();
                if (node.IsLeaf)
                {
                    var c = Utilities.StrToChar(node.C);
                    encodingTable[c] = path;
                }
                else
                {
                    if (node.Left != null) stack.Push((node.Left, path.Append(false).ToArray()));
                    if (node.Right != null) stack.Push((node.Right, path.Append(true).ToArray()));
                }
            }

            return new HuffmanCoder(queue.Max, encodingTable);
        }

        public static byte[] Encode(string text)
        {
            var encoder = HuffmanCoder.CreateFromText(text);
            var signature = encoder.ToSignature();
            var contentLength = text.Length;
            var content = text.SelectMany(c => encoder.EncodingTable[c]);

            var headerBytes = Encoding.UTF8.GetBytes($"{signature}\n{contentLength}\n");
            var contentBytes = Utilities.BoolsToBytes(content);

            return headerBytes.Concat(contentBytes).ToArray();
        }

        public static string Decode(byte[] encodedBytes)
        {
            using (var ms = new MemoryStream(encodedBytes))
            using (var sr = new StreamReader(ms))
            {
                var signature = sr.ReadLine();
                var contentLength = sr.ReadLine();

                var byteCount = Encoding.UTF8.GetByteCount($"{signature}\n{contentLength}\n");

                ms.Position = byteCount;
                var encodedContent = new byte[ms.Length - ms.Position];
                ms.Read(encodedContent, 0, encodedContent.Length);

                var huffman = HuffmanCoder.CreateFromSignature(signature);
                return huffman.Decode(encodedContent, int.Parse(contentLength));
            }
        }

        private string Decode(byte[] encodedContent, int contentLength)
        {
            var content = Utilities.BytesToBools(encodedContent);

            var sb = new StringBuilder();
            var node = this.Root;
            foreach (var bit in content)
            {
                node = bit ? node.Right : node.Left;

                if (node.IsLeaf)
                {
                    sb.Append(Utilities.StrToChar(node.C));
                    node = this.Root;

                    if (sb.Length >= contentLength)
                    {
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        private string ToSignature()
        {
            var leafNodes = this.Root.EnumerateNodes().Where(n => n.IsLeaf);
            return string.Join("|^", leafNodes.Select(n => $"{n.C},^{n.ProbValue}"));
        }
    }

    public class TreeNode
    {
        public readonly static IComparer<TreeNode> Comparer = new TreeNodeComparer();

        public readonly string C;
        public readonly double ProbValue;
        public readonly TreeNode Left;
        public readonly TreeNode Right;

        public TreeNode(char c, double probValue)
        {
            this.C = Utilities.CharToStr(c);
            this.ProbValue = probValue;
            this.Left = null;
            this.Right = null;
        }

        public TreeNode(TreeNode t1, TreeNode t2)
        {
            this.C = string.Concat(t1.C, t2.C); // so we know values further down this branch
            this.ProbValue = t1.ProbValue + t2.ProbValue;
            this.Left = t1;
            this.Right = t2;
        }

        public bool IsLeaf => this.Left == null && this.Right == null;

        public IEnumerable<TreeNode> EnumerateNodes()
        {
            yield return this;

            if (this.IsLeaf)
            {
                yield break;
            }

            foreach (var node in this.Left.EnumerateNodes())
            {
                yield return node;
            }

            foreach (var node in this.Right.EnumerateNodes())
            {
                yield return node;
            }
        }

        public override string ToString()
        {
            return $"{this.C} - {this.ProbValue}";
        }

        private struct TreeNodeComparer : IComparer<TreeNode>
        {
            public int Compare(TreeNode x, TreeNode y)
            {
                if (x == y) return 0;
                if (x == null) return 1;
                if (y == null) return -1;
                if (Math.Abs(x.ProbValue - y.ProbValue) < Math.Abs(x.ProbValue * 0.00001)) return -string.CompareOrdinal(x.C, y.C);

                return x.ProbValue < y.ProbValue ? 1 : -1;
            }
        }
    }

}
