using Encryptics.Cryptography;
using Encryptics.Cryptography.Fips;
using Encryptics.Utils;

namespace Encryptics.WebPortal.Helpers
{
    public class PasswordHasher
    {
        private static readonly IHasherCryptoProvider _hasher = new Sha256CryptoProvider();

        public static string HashPassword(string password)
        {
            return new EncrypticsHashedPassword(_hasher, password).HashedPassword;
        }
    }
}