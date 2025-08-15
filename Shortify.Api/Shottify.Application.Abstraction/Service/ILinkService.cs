using Shortify.Domain.Models;
using Shortify.Infrastructure;

namespace Shottify.Application.Abstraction.Service
{
    public interface ILinkService
    {
        Task<IExecutionResponse> GetLinksAsync();
        Task<IExecutionResponse> GetLinkAsync(Guid linkId);
        Task<IExecutionResponse> GetByUserAsync(string userId);
        Task<IExecutionResponse> AddLinkAsync(LinkEntry linkEntry);
        Task<IExecutionResponse> UpdateLinkAsync(LinkEntry linkEntry);
        Task<IExecutionResponse> RemoveLinkAsync(LinkEntry linkEntry);
    }
}
