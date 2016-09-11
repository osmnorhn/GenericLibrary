using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Generic.Utils.Cryptography
{
    public static class Crypter
    {
        private static string EncryptInternal(string plainText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(initVector);
            byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
            byte[] bytes3 = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations))
            {
                byte[] bytes4 = passwordDeriveBytes.GetBytes(keySize / 8);
                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.Mode = CipherMode.CBC;
                    using (ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes4, bytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(bytes3, 0, bytes3.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] inArray = memoryStream.ToArray();
                                return Convert.ToBase64String(inArray);
                            }
                        }
                    }
                }
            }
        }
        private static string DecryptInternal(string cipherText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(initVector);
            byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
            byte[] array = Convert.FromBase64String(cipherText);
            using (PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations))
            {
                byte[] bytes3 = passwordDeriveBytes.GetBytes(keySize / 8);
                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.Mode = CipherMode.CBC;
                    using (ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes3, bytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(array))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
                            {
                                byte[] array2 = new byte[array.Length];
                                int count = cryptoStream.Read(array2, 0, array2.Length);
                                return Encoding.UTF8.GetString(array2, 0, count);
                            }
                        }
                    }
                }
            }
        }


        private const string PassPhrase = "!G&a(l+a=k#s/i";
        private const string SaltValue = "7e&n1k@va";
        private const string HashAlgorithm = "MD5";
        private const int PasswordIterations = 1;
        private const string InitVector = "#t67hu*wq90@i3ak";
        private const int KeySize = 128;

        public static string Encrypt(string plainText)
        {
            return Crypter.EncryptInternal(plainText, Crypter.PassPhrase, Crypter.SaltValue, Crypter.HashAlgorithm, Crypter.PasswordIterations, Crypter.InitVector, Crypter.KeySize);
        }
        public static string Decrypt(string cipherText)
        {
            return Crypter.DecryptInternal(cipherText, Crypter.PassPhrase, Crypter.SaltValue, Crypter.HashAlgorithm, Crypter.PasswordIterations, Crypter.InitVector, Crypter.KeySize);
        }
    }
}
