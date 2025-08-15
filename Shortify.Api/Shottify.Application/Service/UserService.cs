using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shottify.Application.Abstraction.Service;

namespace Shottify.Application.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User>  _userManager;
        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IExecutionResponse> GetUserAsync(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return ExecutionResponse.Failure("User not found");
            }

            return ExecutionResponse.Successful(user);
        }
        public async Task<IExecutionResponse> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) 
            {
                return ExecutionResponse.Failure("User not found");
            }
            return ExecutionResponse.Successful(user);
        }
        public async Task<IExecutionResponse> CreateUserAsync(User user, string password)
        {
            IdentityResult identityResult = password == null
                ? await _userManager.CreateAsync(user)
                : await _userManager.CreateAsync(user, password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return ExecutionResponse.Failure(errors);
            }

            return ExecutionResponse.Successful(user);
        }

        public async Task<IExecutionResponse> AddToRoleAsync(User user, string role)
        {
            IdentityResult identityResult = await _userManager.AddToRoleAsync(user, role);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return ExecutionResponse.Failure(errors);
            }
            return ExecutionResponse.Successful(user);
        }

        public async Task<IExecutionResponse> UpdateUserAsync(User user)
        {
            IdentityResult updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return ExecutionResponse.Failure(errors);
            }

            return ExecutionResponse.Successful(user.Id);
        }
        public async Task<IExecutionResponse> DeleteUserAsync(User user)
        {
            IdentityResult deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return ExecutionResponse.Failure(errors);
            }

            return ExecutionResponse.Successful(user.Id);
        }
        public async Task<IExecutionResponse> GetRolesAsync(User user)
        {
            var userRole =  await _userManager.GetRolesAsync(user);

            return userRole.Any()
                ? ExecutionResponse.Successful(userRole)
                : ExecutionResponse.Failure("No roles found for this user.");
        }

        public async Task<bool> CheckUserPassword(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}
