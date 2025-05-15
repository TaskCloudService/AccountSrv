

namespace Presentation.Services;

public interface IRefreshTokenService
{
    Task<Guid?> ValidateAsync(string refreshToken);
    Task<string> GenerateAsync(Guid userId);
    Task<string> RotateAsync(string oldToken, Guid userId);
}
