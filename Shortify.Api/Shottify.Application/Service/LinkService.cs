using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Abstraction.Service;

namespace Shottify.Application.Service
{
    public class LinkService : ILinkService
    {
        private readonly ILinkRepository _linkRepository;
        public LinkService(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }
        public async Task<IExecutionResponse> GetLinksAsync()
        {
            return await _linkRepository.GetLinksAsync();
        }
       public async Task<IExecutionResponse> GetLinkAsync(Guid linkId)
        {
            return await _linkRepository.GetLinkAsync(linkId);
        }
        public async Task<IExecutionResponse> GetByUserAsync(string userId) 
        {
            return await _linkRepository.GetByUserAsync(userId); 
        }
        public async Task<IExecutionResponse> AddLinkAsync(LinkEntry linkEntry)
        {
            var existingLinksResponse = await _linkRepository.GetLinksAsync();
            if (!existingLinksResponse.Success)
                return ExecutionResponse.Failure("Failed to retrieve links");

            var links = (IEnumerable<LinkEntry>)existingLinksResponse.Result;

            if (links.Any(x => x.OriginalUrl == linkEntry.OriginalUrl))
                return ExecutionResponse.Failure("This URL already exists");

            return await _linkRepository.AddAsync(linkEntry);
        }
        public async Task<IExecutionResponse> UpdateLinkAsync(LinkEntry linkEntry)
        {
            return await _linkRepository.UpdateAsync(linkEntry);
        }
        public async Task<IExecutionResponse> RemoveLinkAsync(LinkEntry linkEntry)
        {
            return await _linkRepository.RemoveAsync(linkEntry);
        }
    }
}
