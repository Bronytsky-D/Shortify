using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shortify.Application.DTOs;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Abstraction.Service;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shottify.Application.Service
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public TokenService(ITokenRepository tokenRepository,
            IConfiguration configuration,
            IUserService userService)
        {
            _tokenRepository = tokenRepository;
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<IExecutionResponse> GenerateTokensAsync(User user, string role)
        {
            var accessToken = GenerateAccessToken(user, role);
            var refreshToken = GenerateRefreshToken();
            AuthResultDTO TokenResponse = new AuthResultDTO{
                AccessToken = accessToken, 
                RefreshToken =  refreshToken 
            };
    

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _tokenRepository.AddAsync(userToken);

            return ExecutionResponse.Successful(TokenResponse);
        }
        public string GenerateAccessToken(User user, string role)
        {
            var claims = new List<Claim>
             {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                 new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                 new Claim(ClaimTypes.Role, role)
             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSetting:SecurityKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTSetting:ValidIssuer"],
                audience: _configuration["JWTSetting:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<IExecutionResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var tokensResponse = await _tokenRepository.GetTokensByUserAsync(userId);
            var validToken = (tokensResponse.Result as IEnumerable<UserToken>)?
                .FirstOrDefault(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (validToken == null)
                return ExecutionResponse.Failure("Invalid refresh token");

            await _tokenRepository.RevokeTokenAsync(refreshToken);

            var userResp = await _userService.GetUserAsync(userId);
            if (!userResp.Success)
                return ExecutionResponse.Failure("User not found");

            var user = userResp.Result as User;
            var rolesResp = await _userService.GetRolesAsync(user);
            var roles = rolesResp.Result as IEnumerable<string>;
            var role = roles.FirstOrDefault() ?? "User";

            return await GenerateTokensAsync(user, role);
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSetting:SecurityKey"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task<IExecutionResponse> RevokeAllUserTokensAsync(string userId)
        {
            return await _tokenRepository.RevokeAllTokensAsync(userId);
        }

        public async Task<IExecutionResponse> RevokeRefreshTokenAsync(string refreshToken)
        {
           return await _tokenRepository.RevokeTokenAsync(refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var tokensResponse = await _tokenRepository.GetTokensByUserAsync(userId);
            if (!tokensResponse.Success) return false;

            var userTokens = tokensResponse.Result as IEnumerable<UserToken>;
            var token = userTokens?.FirstOrDefault(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            return token != null;
        }
    }
}
