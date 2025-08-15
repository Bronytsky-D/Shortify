using Shortify.Domain.Models;

namespace Shortify.Infrastructure.Repository
{
    public interface ILinkRepository : IRepository<LinkEntry>
    {
        Task<IExecutionResponse> GetByUserAsync(string userId);
        Task<IExecutionResponse> GetLinksAsync();
        Task<IExecutionResponse> GetLinkAsync(Guid id);

    }
}
