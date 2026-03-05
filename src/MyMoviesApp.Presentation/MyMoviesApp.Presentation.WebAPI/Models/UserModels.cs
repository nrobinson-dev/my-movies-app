namespace MyMoviesApp.Presentation.WebAPI.Models;

/// <summary>
/// Request model for creating a user via the API.
/// </summary>
public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model for returning user info via the API.
/// </summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
