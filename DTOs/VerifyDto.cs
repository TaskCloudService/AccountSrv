namespace Presentation.DTOs;

public partial class AuthController
{
    public record VerifyDto(Guid UserId, string Code);
}
