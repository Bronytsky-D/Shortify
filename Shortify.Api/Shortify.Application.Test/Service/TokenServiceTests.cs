using Microsoft.Extensions.Configuration;
using Moq;
using Shortify.Application.DTOs;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Abstraction.Service;
using Shottify.Application.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shortify.Application.Test.Service
{
    public class TokenServiceTests
    {
        private readonly Mock<ITokenRepository> _tokenRepoMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _tokenRepoMock = new Mock<ITokenRepository>();
            _userServiceMock = new Mock<IUserService>();

            var inMemorySettings = new Dictionary<string, string>
            {
                {"JWTSetting:SecurityKey", "super_secret_key_1234567890123456!"},
                {"JWTSetting:ValidIssuer", "testIssuer"},
                {"JWTSetting:ValidAudience", "testAudience"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(
                _tokenRepoMock.Object,
                _config,
                _userServiceMock.Object
            );
        }

        [Fact]
        public async Task GenerateTokensAsync_ShouldSaveRefreshToken()
        {
            var user = new User { Id = "user1", Email = "test@example.com" };

            _tokenRepoMock
                .Setup(r => r.AddAsync(It.IsAny<UserToken>()))
                .ReturnsAsync(ExecutionResponse.Successful(new object()));

            var result = await _tokenService.GenerateTokensAsync(user, "Admin");

            Assert.True(result.Success);
            var tokenResult = Assert.IsType<AuthResultDTO>(result.Result);
            Assert.NotNull(tokenResult.AccessToken);
            Assert.NotNull(tokenResult.RefreshToken);

            _tokenRepoMock.Verify(r => r.AddAsync(It.Is<UserToken>(
                t => t.UserId == "user1" && t.Token == tokenResult.RefreshToken
            )), Times.Once);
        }

        [Fact]
        public void GenerateAccessToken_ShouldContainUserIdAndRole()
        {
            var user = new User { Id = "user123", Email = "test@example.com" };
            var token = _tokenService.GenerateAccessToken(user, "Admin");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("user123", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnBase64String()
        {
            var token = _tokenService.GenerateRefreshToken();
            var bytes = Convert.FromBase64String(token);
            Assert.Equal(32, bytes.Length);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnTrue_WhenValid()
        {
            var refreshToken = "refresh_valid";
            var token = new UserToken
            {
                UserId = "u1",
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };

            _tokenRepoMock
                .Setup(r => r.GetTokensByUserAsync("u1", false))
                .ReturnsAsync(ExecutionResponse.Successful(new[] { token }));

            var isValid = await _tokenService.ValidateRefreshTokenAsync("u1", refreshToken);

            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnFalse_WhenInvalid()
        {
            _tokenRepoMock
                .Setup(r => r.GetTokensByUserAsync("u1", false))
                .ReturnsAsync(ExecutionResponse.Successful(new List<UserToken>()));

            var isValid = await _tokenService.ValidateRefreshTokenAsync("u1", "some_token");

            Assert.False(isValid);
        }
    }
}
