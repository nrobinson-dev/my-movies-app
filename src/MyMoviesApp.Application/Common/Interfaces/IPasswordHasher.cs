namespace MyMoviesApp.Application.Common.Interfaces;

/// <summary>
/// Handles password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password for secure storage.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plaintext password matches a stored hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

