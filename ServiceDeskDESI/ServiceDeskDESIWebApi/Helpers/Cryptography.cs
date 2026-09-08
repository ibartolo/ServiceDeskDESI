using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ServiceDeskDESIWebApi.Helpers
{
    public static class Cryptography
    {
        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return string.Empty;
            }

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        // ====================================================================
        // Hashing de contraseñas (PBKDF2) — reemplaza a Rijndael para contraseñas.
        // ====================================================================

        private const int Pbkdf2Iterations = 10000;

        /// <summary>
        /// Genera el hash PBKDF2 de una contraseña en formato
        /// "PBKDF2$&lt;iterations&gt;$&lt;salt base64&gt;$&lt;hash base64&gt;".
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
            {
                hash = pbkdf2.GetBytes(32);
            }

            return string.Concat("PBKDF2$", Pbkdf2Iterations, "$", Convert.ToBase64String(salt), "$", Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Verifica una contraseña en texto plano contra un valor almacenado que puede
        /// ser un hash PBKDF2 (nuevo) o ciphertext Rijndael (legacy, en transición).
        /// </summary>
        public static bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (stored.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4)
                {
                    return false;
                }

                int iterations;
                if (!int.TryParse(parts[1], out iterations))
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                byte[] actual;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    actual = pbkdf2.GetBytes(expected.Length);
                }

                // Comparación en tiempo constante
                if (actual.Length != expected.Length)
                {
                    return false;
                }
                int diff = 0;
                for (int i = 0; i < actual.Length; i++)
                {
                    diff |= actual[i] ^ expected[i];
                }
                return diff == 0;
            }

            // Fallback legacy: ciphertext Rijndael (usuarios existentes, en transición).
            return string.Equals(Encrypt(password), stored, StringComparison.Ordinal);
        }

        /// <summary>
        /// Genera una contraseña temporal segura y legible (sin caracteres ambiguos).
        /// </summary>
        public static string GeneratePassword(int length = 16)
        {
            const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789!@#$%";
            byte[] rnd = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(rnd);
            }

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[rnd[i] % chars.Length]);
            }
            return sb.ToString();
        }
    }
}