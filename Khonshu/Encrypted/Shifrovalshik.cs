using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Khonshu.Encrypted
{
    public sealed class Shifrovalshik
    {
        private readonly Byte[] key;
        private readonly Byte[] iv;

        public Shifrovalshik(Byte[] key, Byte[] iv)
        {
            this.key = key;
            this.iv = iv;
        }

        public Shifrovalshik(String StrKEY, String StrIV)
        {
            key = Convert.FromBase64String(StrKEY);
            iv = Convert.FromBase64String(StrIV);
        }

        public Shifrovalshik(HelpKhonshu keyAndIv)
        {
            key = keyAndIv.GetKey();
            iv = keyAndIv.GetIV();
        }


        public async Task<String> EncryptAsync(String text, Byte[] UZkey = null, Byte[] UZiv = null)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    if (UZkey != null && UZiv != null)
                    {
                        aes.Key = UZkey;
                        aes.IV = UZiv;
                    }
                    else
                    {
                        aes.Key = key;
                        aes.IV = iv;
                    }



                    ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

                    await using (MemoryStream ms = new MemoryStream())
                    {
                        await using (CryptoStream cp = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                        {
                            await using (StreamWriter sw = new StreamWriter(cp))
                            {
                                await sw.WriteAsync(text);
                            }
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }

            }
            catch
            {
                return text;
            }
        }

        public async Task<String> DecryptAsync(String textShifrovanni, Byte[] UZkey = null, Byte[] UZiv = null)
        {
            try
            {
                Byte[] buffer = Convert.FromBase64String(textShifrovanni);

                using (Aes aes = Aes.Create())
                {
                    if (UZkey != null && UZiv != null)
                    {
                        aes.Key = UZkey;
                        aes.IV = UZiv;
                    }
                    else
                    {
                        aes.Key = key;
                        aes.IV = iv;
                    }

                    ICryptoTransform decrypt = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream cp = new CryptoStream(ms, decrypt, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cp))
                            {
                                return await sr.ReadToEndAsync();
                            }
                        }
                    }
                }
            }
            catch
            {
                return textShifrovanni;
            }
        }
        
        public String Encrypt(String text, Byte[]? uzKey = null, Byte[]? uzIv = null)
        {
            if (string.IsNullOrEmpty(text)) return text;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = uzKey ?? key;
                aes.IV = uzIv ?? iv;

                using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cp = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cp, Encoding.UTF8))
                {
                    sw.Write(text);
                }
                        
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return text;
            }
        }


        public String Decrypt(String textShifrovanni, Byte[]? uzKey = null, Byte[]? uzIv = null)
        {
            if (String.IsNullOrEmpty(textShifrovanni)) return textShifrovanni;

            try
            {
                Byte[] buffer = Convert.FromBase64String(textShifrovanni);

                using Aes aes = Aes.Create();
                aes.Key = uzKey ?? key;
                aes.IV = uzIv ?? iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream(buffer);
                using CryptoStream cp = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cp, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch
            {
                return textShifrovanni;
            }
        }
    }
}
