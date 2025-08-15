using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL.ApplicationContext;
using Shortify.Infrastructure.Repository;


namespace Shortify.Infrastructure.PostgreSQL.Repository
{
    public class LinkRepository : ILinkRepository
    {
        private readonly ApplicationDbContext _context;
        public LinkRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IExecutionResponse> GetLinksAsync()
        {
            var respone = await _context.LinkEntries.ToListAsync();
            if (respone == null || !respone.Any())
            {
                return ExecutionResponse.Failure("No links found.");
            }
            return ExecutionResponse.Successful(respone);
        }
        public async Task<IExecutionResponse> GetLinkAsync(Guid id)
        {
            var respone = await _context.LinkEntries.FirstOrDefaultAsync( link => link.Id == id);
            if (respone == null )
            {
                return ExecutionResponse.Failure("No links found.");
            }
            return ExecutionResponse.Successful(respone);
        }
        public async Task<IExecutionResponse> GetByUserAsync(string userId)
        {
            var links = await _context.LinkEntries
                .Where(link => link.CreatedBy == userId)
                .ToListAsync();
            if (links == null || !links.Any())
            {
                return ExecutionResponse.Failure($"No links found for user with ID {userId}.");
            }
            return ExecutionResponse.Successful(links);
        }
        public async Task<IExecutionResponse> AddAsync(LinkEntry link)
        {
            _context.LinkEntries.Add(link);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(link);
        }
        public async Task<IExecutionResponse> UpdateAsync(LinkEntry link)
        {
            _context.LinkEntries.Update(link);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(link.Id);
        }
        public async Task<IExecutionResponse> RemoveAsync(LinkEntry link)
        {

            _context.LinkEntries.Remove(link);
            await _context.SaveChangesAsync();
            return ExecutionResponse.Successful(link.Id);
        }
    }
}
