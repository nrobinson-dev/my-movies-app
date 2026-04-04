namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Represents a user in the MyMoviesApp domain.
/// </summary>
public class User(Guid id, string email)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
}