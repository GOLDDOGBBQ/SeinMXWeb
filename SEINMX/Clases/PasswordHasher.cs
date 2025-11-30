namespace SEINMX.Clases;


using System;
using System.Security.Cryptography;


public class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    public static string GenerateHash(string password)
    {
        byte[] salt;
        using (var rng = RandomNumberGenerator.Create())
        {
            salt = new byte[SaltSize];
            rng.GetBytes(salt);
        }

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        string base64Hash = Convert.ToBase64String(hashBytes);
        return base64Hash;
    }

    public static bool Verify(string password, string hashedPassword)
    {
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);
        byte[] storedHash = new byte[HashSize];
        Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] testHash = pbkdf2.GetBytes(HashSize);

        for (int i = 0; i < HashSize; i++)
        {
            if (storedHash[i] != testHash[i])
            {
                return false;
            }
        }
        return true;
    }
}