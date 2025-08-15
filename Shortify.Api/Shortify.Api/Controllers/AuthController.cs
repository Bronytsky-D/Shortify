using Microsoft.AspNetCore.Mvc;
using Shortify.Application.DTOs;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shottify.Application.Abstraction.DTOs;
using Shottify.Application.Abstraction.Service;
using System.Data;

namespace Shortify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        public AuthController(IUserService userService,
            ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IExecutionResponse> Login(LoginAuthDTo dto)
        {
            var userRespone = await _userService.GetUserByEmailAsync(dto.Email);
            var user = userRespone.Result as User;

            var result = await _userService.CheckUserPassword(user, dto.Password);

            if (!result)
            {
                return ExecutionResponse.Failure("Invalid Password");
            }

            var roles = (await _userService.GetRolesAsync(user)).Result as IEnumerable<string>; ;
            if (roles == null || !roles.Any())
                return ExecutionResponse.Failure("User has no roles");

            var tokenRespone = await _tokenService.GenerateTokensAsync(user, roles.First());
            AuthResultDTO tokens = (AuthResultDTO)tokenRespone.Result;
            SetRefreshTokenCookie(tokens.RefreshToken);

            AuthResponeseDTO respone = new AuthResponeseDTO
            {
                AccessToken = tokens.AccessToken,
                Email = user.Email,
                UserName = user.UserName,
                Role = roles.FirstOrDefault(),
                UserId = user.Id
            };

            return ExecutionResponse.Successful(respone); 
        }

        [HttpPost("register")]
        public async Task<IExecutionResponse> Register(RegisterAuthDTO dto)
        {
            var newUser = new User { 
                Email = dto.Email,
                UserName = dto.UserName,
            };

            await _userService.CreateUserAsync(newUser, dto.Password);
            await _userService.AddToRoleAsync(newUser, dto.role);

            var role = (await _userService.GetRolesAsync(newUser)).Result as IEnumerable<string>;

            var tokenRespone = await _tokenService.GenerateTokensAsync(newUser, role.First());
            AuthResultDTO tokens = (AuthResultDTO)tokenRespone.Result;
            SetRefreshTokenCookie(tokens.RefreshToken);

            AuthResponeseDTO respone = new AuthResponeseDTO
            {
                AccessToken = tokens.AccessToken,
                Email = newUser.Email,
                UserName = newUser.UserName,
                Role = role.FirstOrDefault(),
                UserId = newUser.Id
            };
            return ExecutionResponse.Successful(respone);
        }

        [HttpPost("refresh")]
        public async Task<IExecutionResponse> RefreshToken([FromBody] RefreshTokenDTO dto)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var tokenRespone = await _tokenService.RefreshTokenAsync(dto.AccessToken, refreshToken);
            AuthResultDTO tokens = (AuthResultDTO)tokenRespone.Result;
            SetRefreshTokenCookie(tokens.RefreshToken);

            return ExecutionResponse.Successful(new { accessToken = tokens.AccessToken });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) 
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
