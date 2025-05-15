using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;
using Presentaion.Data;
using Presentation.Interfaces;

public sealed class VerificationService(
        ApplicationDbContext db,
        IEmailSender email) : IVerificationService
{
    public async Task SendCodeAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (user.EmailConfirmed)
            return;

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();   

        db.EmailVerificationTokens.Add(new EmailVfTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = Hash(code),        
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
            Used = false
        });
        await db.SaveChangesAsync(ct);

        var body = $"<p>Your verification code is <strong>{code}</strong>. " +
                   $"It expires in 15 minutes.</p>";
        await email.SendAsync(user.Email!, "Verify your account", body, ct);
    }

    public async Task<bool> VerifyCodeAsync(Guid userId, string plainCode, CancellationToken ct = default)
    {
        var token = await db.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.Used && t.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(t => t.ExpiresAtUtc)
            .FirstOrDefaultAsync(ct);

        if (token is null) return false;
        if (token.Code != Hash(plainCode)) return false;          

        token.Used = true;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string Hash(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }
}
