using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MeshBackend.Helpers
{
    public static class PasswordCheckHelper
    {
        public static HashPassword GetHashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string passwordSalt = Convert.ToBase64String(salt);
            string passwordDigest = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));

            return new HashPassword()
            {
                PasswordSalt = passwordSalt,
                PasswordDigest = passwordDigest
            };
        }

        public static bool CheckHashPassword(string password, string passwordSalt, string passwordDigest)
        {
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(passwordSalt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));
            return hashed == passwordDigest;
        }
        


        public class HashPassword
        {
            public string PasswordSalt { get; set; }
            public string PasswordDigest { get;set; }
        }
    }
}