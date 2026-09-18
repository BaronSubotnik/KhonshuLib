using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Pkcs;

namespace Khonshu.Encrypted
{
    public sealed class HelpKhonshu
    {
        private readonly String yourKey;
        private Byte[] salt; 

        private  Byte[] KEY;
        private  Byte[] IV;

        public HelpKhonshu(String yourKey, String salt)
        {
            this.yourKey = yourKey;
            this.salt = Encoding.UTF8.GetBytes(salt);

            Generated();
        }
        private void Generated()
        {
            using var niga = new Rfc2898DeriveBytes(password: yourKey, salt: salt,100000,HashAlgorithmName.SHA256);
            KEY = niga.GetBytes(32);
            IV = niga.GetBytes(16);
        }

        public Byte[] GetKey()
        {
            return KEY;
        }
        public Byte[] GetIV()
        {
            return IV;
        }
    }
}
