using System;
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 16 bytes for salt
    private const int HashSize = 32; // 32 bytes for the hash
    private const int Iterations = 10000; // Number of iterations for PBKDF2

    public static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

        // Create a PBKDF2 hash of the password
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Combine the salt and hash
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert the bytes to a base64-encoded string
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // Convert the base64-encoded string back to bytes
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        // Get the salt from the stored hash
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Compute a new hash from the provided password and stored salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] newHash = pbkdf2.GetBytes(HashSize);

        // Compare the newly computed hash with the stored hash
        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != newHash[i])
            {
                return false; // Passwords don't match
            }
        }

        return true; // Passwords match
    }
}
