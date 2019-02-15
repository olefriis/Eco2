using System;
using System.Linq;
using System.Text;

namespace Eco2.Parsing
{
    public static class Conversion
    {
        public static byte[] HexStringToByteArray(string hex)
        {
            var strippedHex = hex.Replace("-", "");
            return Enumerable.Range(0, strippedHex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => (byte)Convert.ToByte(strippedHex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        public static byte[] BigToLittleEndian(byte[] input)
        {
            var output = new byte[input.Length];

            for (int i=0; i<input.Length; i++)
            {
                var wordOffset = (i / 4) * 4;
                var offsetWithinWord = i % 4;
                output[i] = input[wordOffset + (3 - offsetWithinWord)];
            }

            return output;
        }
    }
}
