using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Text;

namespace REST_API.Processings.Cryptography
{
    public class Cryptography
    {
        public string GetPBKDF2Hash(string value, string salt = "3s&YM@B$ecQo8ay$")
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        }
    }
}