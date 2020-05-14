using System.Collections.Generic;

namespace Huffman
{

    public static class Utilities
    {
        public static string CharToStr(char c)
        {
            if (c == '\n') return @"\n";
            if (c == '\r') return @"\r";

            return c.ToString();
        }

        public static char StrToChar(string s)
        {
            if (s == @"\n") return '\n';
            if (s == @"\r") return '\r';

            return char.Parse(s);
        }

        public static byte[] BoolsToBytes(IEnumerable<bool> bools)
        {
            var bytes = new List<byte>();
            byte currByte = 0x0;
            var bitCounter = 0;

            foreach (var b in bools)
            {
                currByte <<= 1;
                bitCounter++;

                if (b)
                {
                    currByte |= 0x1;
                }

                if (bitCounter == 8)
                {
                    bytes.Add(currByte);
                    currByte = 0x0;
                    bitCounter = 0;
                }
            }

            if (bitCounter != 0)
            {
                bytes.Add(currByte <<= (8 - bitCounter));
            }

            return bytes.ToArray();
        }

        public static IEnumerable<bool> BytesToBools(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    yield return (b & (1 << i)) > 0;
                }
            }
        }
    }
}
