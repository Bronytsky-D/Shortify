using Shortify.Infrastructure;

namespace Shottify.Application.Abstraction.Service
{
    public interface ILinkShortenerService
    {
        Task<IExecutionResponse> CreateShortLinkAsync(string originalUrl, string? userId = null, int codeLength = 6);
        Task<IExecutionResponse> GetOriginalUrlAsync(string shortCode);
    }
}
