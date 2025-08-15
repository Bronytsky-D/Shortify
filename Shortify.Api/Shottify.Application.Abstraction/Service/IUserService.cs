using Shortify.Domain.Models;
using Shortify.Infrastructure;

namespace Shottify.Application.Abstraction.Service
{
    public interface IUserService
    {
        Task<IExecutionResponse> GetUserAsync(string id);
        Task<IExecutionResponse> GetUserByEmailAsync(string email);
        Task<IExecutionResponse> CreateUserAsync(User user, string password);
        Task<IExecutionResponse> UpdateUserAsync(User user);
        Task<IExecutionResponse> DeleteUserAsync(User user);
        Task<IExecutionResponse> AddToRoleAsync(User user, string role);
        Task<bool> CheckUserPassword(User user, string password);
        Task<IExecutionResponse> GetRolesAsync(User user);
    }
}
