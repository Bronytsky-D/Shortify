using Shortify.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortify.Infrastructure.Test.Utils
{
    public static class LinkFactory
    {
        public static LinkEntry CreateLink(string userId)
        {
            return new LinkEntry
            {
                CreatedBy = userId,
                OriginalUrl = "https://example.com",
                ShortenedUrl = "short123",
                Title = "Test title",
                Description = "Test description",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
