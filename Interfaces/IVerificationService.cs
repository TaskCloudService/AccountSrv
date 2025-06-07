using Presentation.Models;

public interface IVerificationService
{
    Task SendCodeAsync(ApplicationUser user, CancellationToken ct = default);
    Task<bool> VerifyCodeAsync(Guid userId, string plainCode, CancellationToken ct = default);
}
