using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Represents a user in the MyMoviesApp domain.
/// </summary>
public class User(Guid id, string email)
{
    public Guid Id { get; } = id;
    
    [Required]
    [EmailAddress]
    public string Email { get; } = email;
}
