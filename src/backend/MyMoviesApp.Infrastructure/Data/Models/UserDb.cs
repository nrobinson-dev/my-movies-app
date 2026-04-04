using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Infrastructure.Data.Models;

/// <summary>
/// Data model for UserDb entity in the database.
/// </summary>
public class UserDb
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
