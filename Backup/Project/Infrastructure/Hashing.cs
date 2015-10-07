using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

namespace Project.Infrastructure
{
    public class Hashing
    {
        public static string CreateHash(string password)
        {
            Encoder enc = Encoding.Unicode.GetEncoder();

            byte[] unicodeText = new byte[password.Length * 2];

            enc.GetBytes(password.ToCharArray(), 0, password.Length, unicodeText, 0, true);

            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] result = md5.ComputeHash(unicodeText);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        public static string GeneratePassword()
        {
            char[] Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*(){}".ToCharArray();

            string Password = null;

            Random rnd = new Random();

            for (int i = 0; i < 7; i++)
            {
                Password += Characters[rnd.Next(72)].ToString();
            }

            return Password;
        }
    }
}