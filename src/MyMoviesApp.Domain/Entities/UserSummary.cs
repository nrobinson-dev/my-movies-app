namespace MyMoviesApp.Domain.Entities;

/// <summary>
/// Used for returning user info without sensitive data.
/// </summary>
public class UserSummary
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
