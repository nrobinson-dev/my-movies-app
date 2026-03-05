using System.Security.Cryptography;
using MyMoviesApp.Application.Common.Interfaces;

namespace MyMoviesApp.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: HashSize);

        var hashWithSalt = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
        Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

        return Convert.ToBase64String(hashWithSalt);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashWithSalt = Convert.FromBase64String(hash);

            var salt = new byte[SaltSize];
            Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

            var hashOfInput = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashWithSalt[i + SaltSize] != hashOfInput[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}