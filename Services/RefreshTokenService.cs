
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Presentaion.Data;
using Presentation.Models;

// Code researched on StackOverflow

namespace Presentation.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _db;
        public RefreshTokenService(ApplicationDbContext db) => _db = db;

        public async Task<Guid?> ValidateAsync(string refreshToken)
        {
            var entry = await _db.RefreshTokens
                .SingleOrDefaultAsync(t => t.Token == refreshToken
                    && !t.Revoked
                    && t.ExpiresAt > DateTime.UtcNow);
            return entry?.UserId;
        }

        public async Task<string> GenerateAsync(Guid userId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var entry = new RefreshTokenEntity
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            };
            _db.RefreshTokens.Add(entry);
            await _db.SaveChangesAsync();
            return token;
        }

        public async Task<string> RotateAsync(string oldToken, Guid userId)
        {
            var existing = await _db.RefreshTokens.SingleAsync(t => t.Token == oldToken);
            existing.Revoked = true;
            await _db.SaveChangesAsync();
            return await GenerateAsync(userId);
        }
    }
}
