namespace Presentation.DTOs;

public partial class AuthController
{
    public record LoginDto(string Email, string Password);
}
