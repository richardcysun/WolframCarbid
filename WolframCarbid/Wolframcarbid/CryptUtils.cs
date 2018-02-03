using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;

//Below codes mostly come from MSDN example of AesCryptoServiceProvider
namespace Wolframcarbid
{
    static class CWCCrypt
    {
        private static byte[] m_key;
        private static byte[] m_iv;

        static CWCCrypt()
        {
            m_key = Encoding.ASCII.GetBytes("1234567887654321");
            m_iv = Encoding.ASCII.GetBytes("8765432112345678");
        }

        static public string Encrypt(string strPlainTexts)
        {
            string strBase64 = "";
            try
            {
                AesCryptoServiceProvider myAes = new AesCryptoServiceProvider();

                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(strPlainTexts, m_key, m_iv);
                strBase64 = Convert.ToBase64String(encrypted);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Encrypt Error: {0}", e.Message);
            }

            return strBase64;
        }

        static public string Decrypt(string strCipherText)
        {
            string strPlainText = "";
            try
            {
                AesCryptoServiceProvider myAes = new AesCryptoServiceProvider();

                // Decrypt the bytes to a string.
                byte[] fromBase64 = Convert.FromBase64String(strCipherText);
                strPlainText = DecryptStringFromBytes_Aes(fromBase64, m_key, m_iv);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Decrypt Error: {0}", e.Message);
            }
            return strPlainText;
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}
