using System;
using System.Text;
using Eco2.Encryption;
using Eco2.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eco2Tests.Encryption
{
    [TestClass]
    public class EncryptionTests
    {
        const string key = "DF-5B-7D-6A-16-32-CC-A4-79-30-6E-B3-78-B6-E9-59";

        [TestMethod]
        public void CanDecode()
        {
            var encryptedName = "8F-EA-76-86-55-22-A2-82-F1-AB-F1-E4-17-1C-B0-02";

            var bytes = Encoding.ASCII.GetBytes("Tilbygning");
            Array.Resize(ref bytes, 16); // We've got 16-byte blocks

            var decryptedName = new Eco2.Encryption.Encryption(key).Decrypt(encryptedName);

            Assert.AreEqual(Conversion.ByteArrayToString(bytes), Conversion.ByteArrayToString(decryptedName));
        }

        [TestMethod]
        public void CanEncode()
        {
            var xxTea = new Eco2.Encryption.Encryption(key);

            var bytes = Encoding.ASCII.GetBytes("Tilbygning");
            Array.Resize(ref bytes, 16); // We've got 16-byte blocks

            var encryptedName = xxTea.Encrypt(bytes);
            Assert.AreEqual("8F-EA-76-86-55-22-A2-82-F1-AB-F1-E4-17-1C-B0-02", encryptedName);
        }
    }
}
