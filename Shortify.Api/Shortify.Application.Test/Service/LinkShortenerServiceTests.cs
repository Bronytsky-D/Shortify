using Moq;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Service;

namespace Shortify.Application.Test.Service
{
    public class LinkShortenerServiceTests
    {
        private readonly Mock<ILinkRepository> _linkRepoMock;
        private readonly LinkShortenerService _shortenerService;

        public LinkShortenerServiceTests()
        {
            _linkRepoMock = new Mock<ILinkRepository>();
            _shortenerService = new LinkShortenerService(_linkRepoMock.Object);
        }

        [Fact]
        public async Task CreateShortLinkAsync_ShouldFail_WhenUrlIsInvalid()
        {
            var result = await _shortenerService.CreateShortLinkAsync("invalid_url");

            Assert.False(result.Success);
            Assert.Equal("Invalid URL", result.Errors.First());
        }

        [Fact]
        public async Task CreateShortLinkAsync_ShouldReturnShortCode_WhenUrlIsValid()
        {
            _linkRepoMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(new List<LinkEntry>()));

            var result = await _shortenerService.CreateShortLinkAsync("https://example.com");

            Assert.True(result.Success);
            var code = Assert.IsType<string>(result.Result);
            Assert.Equal(6, code.Length);
        }

        [Fact]
        public async Task CreateShortLinkAsync_ShouldAvoidDuplicateCodes()
        {
            var existingLink = new LinkEntry { ShortenedUrl = "abc123" };
            _linkRepoMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(new List<LinkEntry> { existingLink }));

            var result = await _shortenerService.CreateShortLinkAsync("https://example.com", codeLength: 6);

            Assert.True(result.Success);
            var code = Assert.IsType<string>(result.Result);
            Assert.NotEqual("abc123", code);
        }

        [Fact]
        public async Task GetOriginalUrlAsync_ShouldReturnOriginalUrl_WhenLinkExists()
        {
            var link = new LinkEntry { ShortenedUrl = "abc123", OriginalUrl = "https://example.com" };
            _linkRepoMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(new List<LinkEntry> { link }));

            var result = await _shortenerService.GetOriginalUrlAsync("abc123");

            Assert.True(result.Success);
            var original = Assert.IsType<string>(result.Result);
            Assert.Equal("https://example.com", original);
        }

        [Fact]
        public async Task GetOriginalUrlAsync_ShouldFail_WhenLinkNotFound()
        {
            _linkRepoMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(new List<LinkEntry>()));

            var result = await _shortenerService.GetOriginalUrlAsync("missing");

            Assert.False(result.Success);
            Assert.Equal("Link not found.", result.Errors.First());
        }

        [Fact]
        public async Task GetOriginalUrlAsync_ShouldFail_WhenRepositoryFails()
        {
            _linkRepoMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Failure("DB error"));

            var result = await _shortenerService.GetOriginalUrlAsync("anycode");

            Assert.False(result.Success);
            Assert.Equal("No links found.", result.Errors.First());
        }
    }
}
