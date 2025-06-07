namespace Presentation.DTOs;

public partial class AuthController
{
    public record RegisterDto(string Email, string Password, string FirstName, string LastName);
}
