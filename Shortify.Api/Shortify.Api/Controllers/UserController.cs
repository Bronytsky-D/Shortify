using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shottify.Application.Abstraction.DTOs;
using Shottify.Application.Abstraction.Service;

namespace Shortify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IExecutionResponse> GetUser(string id)
        {
            var userRespone =  await _userService.GetUserAsync(id);
            var user = (User)userRespone.Result;
            GetUserDTO userInfo = new GetUserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            return ExecutionResponse.Successful(userInfo);
        }

    }
}
