using Eco2.Parsing;

namespace Eco2.Encryption
{
    /*
     * Mostly copied from https://github.com/dsltip/Danfoss-BLE
     */
    public class Encryption
    {
        readonly byte[] key;

        public Encryption(string key)
        {
            this.key = Conversion.HexStringToByteArray(key);
        }

        public byte[] Decrypt(string encryptedHexValue)
        {
            var data = Conversion.HexStringToByteArray(encryptedHexValue);
            var shuffledData = Conversion.BigToLittleEndian(data);

            return Conversion.BigToLittleEndian(XXTEA.Decrypt(shuffledData, key));
        }

        public string Encrypt(byte[] data)
        {
            var shuffledData = Conversion.BigToLittleEndian(data);
            var encrypted = Conversion.BigToLittleEndian(XXTEA.Encrypt(shuffledData, key));
            return Conversion.ByteArrayToString(encrypted);
        }
    }
}
