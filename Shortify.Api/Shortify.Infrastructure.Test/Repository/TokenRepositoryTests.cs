using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL.ApplicationContext;
using Shortify.Infrastructure.PostgreSQL.Repository;


namespace Shortify.Infrastructure.Test.Repository
{
    public class TokenRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserToken CreateToken(string userId, bool revoked = false, DateTime? expires = null)
        {
            return new UserToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                IsRevoked = revoked,
                ExpiresAt = expires ?? DateTime.UtcNow.AddHours(1)
            };
        }

        [Fact]
        public async Task AddAsync_ShouldAddToken()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1");
            var result = await repo.AddAsync(token);

            Assert.True(result.Success);
            Assert.NotNull(result.Result);
            Assert.Single(context.UserTokens);
        }

        [Fact]
        public async Task GetTokensByUserAsync_ShouldReturnOnlyActiveTokens()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            await repo.AddAsync(CreateToken("user1"));
            await repo.AddAsync(CreateToken("user1", revoked: true));

            var result = await repo.GetTokensByUserAsync("user1");

            Assert.True(result.Success);
            var tokens = Assert.IsAssignableFrom<IEnumerable<UserToken>>(result.Result);
            Assert.Single(tokens);
            Assert.False(tokens.First().IsRevoked);
        }

        [Fact]
        public async Task GetTokensByUserAsync_WithIncludeRevoked_ShouldReturnAllTokens()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            await repo.AddAsync(CreateToken("user1"));
            await repo.AddAsync(CreateToken("user1", revoked: true));

            var result = await repo.GetTokensByUserAsync("user1", includeRevoked: true);

            Assert.True(result.Success);
            var tokens = Assert.IsAssignableFrom<IEnumerable<UserToken>>(result.Result);
            Assert.Equal(2, tokens.Count());
        }

        [Fact]
        public async Task GetTokensByUserAsync_WhenNoTokens_ShouldReturnFailure()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var result = await repo.GetTokensByUserAsync("userX");

            Assert.False(result.Success);
            Assert.Contains("No tokens found", result.Errors.First());
        }

        [Fact]
        public async Task IsTokenRevokedAsync_WhenTokenNotFound_ShouldReturnTrue()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var isRevoked = await repo.IsTokenRevokedAsync("nonexistent");

            Assert.True(isRevoked);
        }

        [Fact]
        public async Task IsTokenRevokedAsync_WhenTokenExpired_ShouldReturnTrue()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1", expires: DateTime.UtcNow.AddMinutes(-1));
            await repo.AddAsync(token);

            var isRevoked = await repo.IsTokenRevokedAsync(token.Token);

            Assert.True(isRevoked);
        }

        [Fact]
        public async Task IsTokenRevokedAsync_WhenTokenRevoked_ShouldReturnTrue()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1", revoked: true);
            await repo.AddAsync(token);

            var isRevoked = await repo.IsTokenRevokedAsync(token.Token);

            Assert.True(isRevoked);
        }

        [Fact]
        public async Task IsTokenRevokedAsync_WhenTokenValid_ShouldReturnFalse()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1");
            await repo.AddAsync(token);

            var isRevoked = await repo.IsTokenRevokedAsync(token.Token);

            Assert.False(isRevoked);
        }

        [Fact]
        public async Task RevokeAllTokensAsync_ShouldRevokeOnlyActiveTokens()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            await repo.AddAsync(CreateToken("user1"));
            await repo.AddAsync(CreateToken("user1", revoked: true));

            var result = await repo.RevokeAllTokensAsync("user1");

            Assert.True(result.Success);
            Assert.All(context.UserTokens, t => Assert.True(t.IsRevoked));
        }

        [Fact]
        public async Task RevokeTokenAsync_ShouldRevokeSpecificToken()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1");
            await repo.AddAsync(token);

            var result = await repo.RevokeTokenAsync(token.Token);

            Assert.True(result.Success);
            Assert.True(context.UserTokens.First().IsRevoked);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateToken()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1");
            await repo.AddAsync(token);

            token.IsRevoked = true;
            var updateResult = await repo.UpdateAsync(token);

            Assert.True(updateResult.Success);
            Assert.True(context.UserTokens.First().IsRevoked);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveToken()
        {
            var context = GetInMemoryDbContext();
            var repo = new TokenRepository(context);

            var token = CreateToken("user1");
            await repo.AddAsync(token);

            var removeResult = await repo.RemoveAsync(token);

            Assert.True(removeResult.Success);
            Assert.Empty(context.UserTokens);
        }
    }
}
