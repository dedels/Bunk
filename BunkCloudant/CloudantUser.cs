using Bunk.CouchBuiltins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Cloudant
{
    public static class CloudantUser 
    {
        public static void SetPassword(this User u, string password, string salt=null)
        {
            var sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            if (salt == null)
            {
                byte[] salt_bytes = new byte[10];
                new Random().NextBytes(salt_bytes);
                salt = Convert.ToBase64String(salt_bytes).Substring(1, 10);
            }
            u.Salt = salt;
            u.PasswordSha = password + u.Salt;

            byte[] password_bytes = Encoding.UTF8.GetBytes(u.PasswordSha);
            password_bytes = sha.ComputeHash(password_bytes);

            u.PasswordSha = BitConverter.ToString(password_bytes);
            u.PasswordSha = u.PasswordSha.Replace("-", "").ToLower();
            u.Password = null;
        }
    }
}
