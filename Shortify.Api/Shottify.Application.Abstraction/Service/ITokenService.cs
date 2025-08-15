using Shortify.Domain.Models;
using Shortify.Infrastructure;
using System.Security.Claims;


namespace Shottify.Application.Abstraction.Service
{
    public interface ITokenService
    {
        Task<IExecutionResponse> GenerateTokensAsync(User user, string role);
        string GenerateAccessToken(User user, string role);
        string GenerateRefreshToken();
        Task<IExecutionResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task<IExecutionResponse> RevokeRefreshTokenAsync(string refreshToken);
        Task<IExecutionResponse> RevokeAllUserTokensAsync(string userId);

    }
}
