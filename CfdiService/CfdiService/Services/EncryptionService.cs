using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CfdiService.Services
{
    public static class EncryptionService
    {
        /// <summary>
        /// TODO: Delete this later.  Only for company users until i get all completed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public static string Sha256_hash(string value)
        //{
        //    return Sha256_hash(value, string.Empty);
        //}

        public static string GenerateSecurityCode()
        {
            Random rand = new Random(DateTime.Now.Second);
            return rand.Next(9999).ToString();
        }

        public static string Sha256_hash(string value, string salt)
        {
            var sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(salt + value));

                foreach (var b in result)
                    sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}