using System.IO;
using System.Reflection;

namespace Huffman
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var inputFile = Path.Combine(path, "ulysses.txt");
            var encodedOutputFile = Path.Combine(path, "ulysses-encoded.txt");
            var decodedOutputFile = Path.Combine(path, "ulysses-decoded.txt");

            var ulysses = File.ReadAllText(inputFile);

            // Encode the input file
            var encoded = HuffmanCoder.Encode(ulysses);
            File.WriteAllBytes(encodedOutputFile, encoded);

            // Read in the output file and ensure it can be decoded
            var decoded = HuffmanCoder.Decode(File.ReadAllBytes(encodedOutputFile));
            File.WriteAllText(decodedOutputFile, decoded);

            // Returns true yay
            var same = string.Equals(ulysses, decoded);
        }
    }
}
