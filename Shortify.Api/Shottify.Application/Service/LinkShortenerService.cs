using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Abstraction.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shottify.Application.Service
{
    public class LinkShortenerService: ILinkShortenerService
    {
        private readonly ILinkRepository _linkRepository;
        private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        public LinkShortenerService(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }

        private string ToBase62(byte[] bytes, int length)
        {
            var value = new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
            var sb = new StringBuilder();
            while (sb.Length < length)
            {
                int idx = (int)(value % 62);
                sb.Append(Base62Chars[idx]);
                value /= 62;
            }
            return sb.ToString();
        }

        private string GenerateRandomCode(int length = 6)
        {
            var buf = new byte[8];
            Rng.GetBytes(buf);
            return ToBase62(buf, length);
        }

        public async Task<IExecutionResponse> CreateShortLinkAsync(string originalUrl, string? userId = null, int codeLength = 6)
        {
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                return ExecutionResponse.Failure("Invalid URL");
            int NumbOfAttention = 6; 
            for (int attempt = 0; attempt < NumbOfAttention; attempt++)
            {
                var code = GenerateRandomCode(codeLength);

                var existing = await _linkRepository.GetLinksAsync();
                var links = (IEnumerable<LinkEntry>)existing.Result;
                if (existing.Success && links.Any(x => x.ShortenedUrl == code))
                    continue;

                var entry = new LinkEntry
                {
                    OriginalUrl = originalUrl,
                    ShortenedUrl = code,
                    CreatedBy = userId
                };

                return ExecutionResponse.Successful(code);
            }

            return ExecutionResponse.Failure("Failed to generate a unique code. Please increase the code length.");
        }

        public async Task<IExecutionResponse> GetOriginalUrlAsync(string shortCode)
        {
            var allLinksRespone = await _linkRepository.GetLinksAsync();
            if (!allLinksRespone.Success)
                return ExecutionResponse.Failure("No links found.");
            var allLinks = (IEnumerable<LinkEntry>)allLinksRespone.Result;
            var link = allLinks.FirstOrDefault(x => x.ShortenedUrl == shortCode && !x.IsDeleted);
            if (link == null)
                return ExecutionResponse.Failure("Link not found.");

            return ExecutionResponse.Successful(link.OriginalUrl);
        }
    }
}
