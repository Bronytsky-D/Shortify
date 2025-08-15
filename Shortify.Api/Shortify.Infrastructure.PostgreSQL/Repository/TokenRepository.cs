using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL.ApplicationContext;
using Shortify.Infrastructure.Repository;

namespace Shortify.Infrastructure.PostgreSQL.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;
        public TokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IExecutionResponse> AddAsync(UserToken entity)
        {
            _context.UserTokens.Add(entity);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(entity);
        }

        public async Task<IExecutionResponse> GetTokensByUserAsync(string userId, bool includeRevoked = false)
        {
            var tokens = await _context.UserTokens
                .Where(token => token.UserId == userId && (includeRevoked || !token.IsRevoked))
                .ToListAsync();
            if(tokens == null || !tokens.Any())
            {
                return ExecutionResponse.Failure($"No tokens found for user with ID {userId}.");
            }
            return ExecutionResponse.Successful(tokens);
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            var tokenEntry = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (tokenEntry == null)
                return true; 

            return tokenEntry.IsRevoked || tokenEntry.ExpiresAt <= DateTime.UtcNow;
        }

        public async Task<IExecutionResponse> RevokeAllTokensAsync(string userId)
        {
            var tokens = await _context.UserTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(tokens);
        }

        public async Task<IExecutionResponse> RevokeTokenAsync(string token)
        {
            var tokenEntry = await _context.UserTokens
            .FirstOrDefaultAsync(t => t.Token == token);

            if (tokenEntry != null)
            {
                tokenEntry.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
            return ExecutionResponse.Successful(tokenEntry.Token);
        }

        public async Task<IExecutionResponse> UpdateAsync(UserToken entity)
        {
            _context.UserTokens.Update(entity);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(entity.Id);
        }
        public async Task<IExecutionResponse> RemoveAsync(UserToken entity)
        {
            _context.UserTokens.Remove(entity);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(entity.Id);
        }
    }
}
