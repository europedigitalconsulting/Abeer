using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abeer.Shared.Technical
{
    public class CryptHelper
    {
        public class Rijndael
        {
            #region Consts
            /// <summary>
            /// Change this Inputkey GUID with a new GUID when you use this code in your own program !!!
            /// Keep this inputkey very safe and prevent someone from decoding it some way !!!
            /// </summary>

            internal static readonly string salt = "8BCF6348-839F-4A2C-8DBC-EF9418BF0FD9";
            static readonly char[] padding = { '=' };
            #endregion

            #region Encryption
            /// <summary>
            /// Encrypt the given text and give the byte array back as a BASE64 string
            /// </summary>
            /// <param name="text">The text to encrypt</param>
            /// <param name="salt">The pasword salt</param>
            /// <returns>The encrypted text</returns>
            public static string Encrypt(string text, string inputKey)
            {
                if (string.IsNullOrEmpty(text))
                    throw new ArgumentNullException(nameof(text));

                var aesAlg = NewRijndaelManaged(inputKey);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                var msEncrypt = new MemoryStream();

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                    swEncrypt.Write(text);


                return Convert.ToBase64String(msEncrypt.ToArray()).TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            }
            #endregion

            #region Decrypt
            /// <summary>
            /// Checks if a string is base64 encoded
            /// </summary>
            /// <param name="base64String">The base64 encoded string</param>
            /// <returns></returns>
            private static bool IsBase64String(string base64String)
            {
                base64String = base64String.Trim();

                return (base64String.Length % 4 == 0) &&
                        Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
            }

            /// <summary>
            /// Decrypts the given text
            /// </summary>
            /// <param name="cipherText">The encrypted BASE64 text</param>
            /// <param name="salt">The pasword salt</param>
            /// <returns>De gedecrypte text</returns>
            public static string Decrypt(string cipherTextBase64SafeUrl, string inputKey)
            {
                if (string.IsNullOrEmpty(cipherTextBase64SafeUrl))
                    throw new ArgumentNullException(nameof(cipherTextBase64SafeUrl));

                string cipherText = cipherTextBase64SafeUrl.Replace('_', '/').Replace('-', '+');
                switch (cipherTextBase64SafeUrl.Length % 4)
                {
                    case 2:
                        cipherText += "=="; break;
                    case 3:
                        cipherText += "="; break;
                }

                if (!IsBase64String(cipherText))
                    throw new Exception("The cipherText input parameter is not base64 encoded");

                var aesAlg = NewRijndaelManaged(inputKey);
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var cipher = Convert.FromBase64String(cipherText);


                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                    return srDecrypt.ReadToEnd();
            }
            #endregion

            #region NewRijndaelManaged
            /// <summary>
            /// Create a new RijndaelManaged class and initialize it
            /// </summary>
            /// <param name="salt">The pasword salt</param>
            /// <returns></returns>
            private static RijndaelManaged NewRijndaelManaged(string inputKey)
            {
                if (salt == null) throw new ArgumentNullException(nameof(salt));
                var saltBytes = Encoding.ASCII.GetBytes(salt);
                var key = new Rfc2898DeriveBytes(inputKey, saltBytes);

                var aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                return aesAlg;
            }
            #endregion
        }
    }
}
