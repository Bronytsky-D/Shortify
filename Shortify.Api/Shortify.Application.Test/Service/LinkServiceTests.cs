using Moq;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Service;


namespace Shortify.Application.Test.Service
{
    public class LinkServiceTests
    {
        private readonly Mock<ILinkRepository> _linkRepositoryMock;
        private readonly LinkService _linkService;

        public LinkServiceTests()
        {
            _linkRepositoryMock = new Mock<ILinkRepository>();
            _linkService = new LinkService(_linkRepositoryMock.Object);
        }

        [Fact]
        public async Task GetLinksAsync_ShouldReturnLinks()
        {
            // Arrange
            var links = new List<LinkEntry> { new LinkEntry { OriginalUrl = "https://a.com" } };
            _linkRepositoryMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(links));

            // Act
            var result = await _linkService.GetLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(links, result.Result);
            _linkRepositoryMock.Verify(r => r.GetLinksAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLinkAsync_ShouldReturnSingleLink()
        {
            var id = Guid.NewGuid();
            var link = new LinkEntry { Id = id, OriginalUrl = "https://a.com" };

            _linkRepositoryMock
                .Setup(r => r.GetLinkAsync(id))
                .ReturnsAsync(ExecutionResponse.Successful(link));

            var result = await _linkService.GetLinkAsync(id);

            Assert.True(result.Success);
            Assert.Equal(link, result.Result);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturnUserLinks()
        {
            var links = new List<LinkEntry> { new LinkEntry { CreatedBy = "user1" } };

            _linkRepositoryMock
                .Setup(r => r.GetByUserAsync("user1"))
                .ReturnsAsync(ExecutionResponse.Successful(links));

            var result = await _linkService.GetByUserAsync("user1");

            Assert.True(result.Success);
            var resultLinks = Assert.IsAssignableFrom<IEnumerable<LinkEntry>>(result.Result);
            Assert.Single(resultLinks);
        }

        [Fact]
        public async Task AddLinkAsync_WhenUrlAlreadyExists_ShouldReturnFailure()
        {
            var link = new LinkEntry { OriginalUrl = "https://dup.com" };
            var existingLinks = new List<LinkEntry> { new LinkEntry { OriginalUrl = "https://dup.com" } };

            _linkRepositoryMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(existingLinks));

            var result = await _linkService.AddLinkAsync(link);

            Assert.False(result.Success);
            Assert.Equal("This URL already exists", result.Errors.First());
            _linkRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LinkEntry>()), Times.Never);
        }

        [Fact]
        public async Task AddLinkAsync_WhenUrlIsUnique_ShouldCallAddAsync()
        {
            var link = new LinkEntry { OriginalUrl = "https://unique.com" };

            _linkRepositoryMock
                .Setup(r => r.GetLinksAsync())
                .ReturnsAsync(ExecutionResponse.Successful(new List<LinkEntry>()));

            _linkRepositoryMock
                .Setup(r => r.AddAsync(link))
                .ReturnsAsync(ExecutionResponse.Successful(link));

            var result = await _linkService.AddLinkAsync(link);

            Assert.True(result.Success);
            _linkRepositoryMock.Verify(r => r.AddAsync(link), Times.Once);
        }

        [Fact]
        public async Task UpdateLinkAsync_ShouldCallRepositoryUpdate()
        {
            var link = new LinkEntry { Id = Guid.NewGuid() };

            _linkRepositoryMock
                .Setup(r => r.UpdateAsync(link))
                .ReturnsAsync(ExecutionResponse.Successful(link));

            var result = await _linkService.UpdateLinkAsync(link);

            Assert.True(result.Success);
            _linkRepositoryMock.Verify(r => r.UpdateAsync(link), Times.Once);
        }

        [Fact]
        public async Task RemoveLinkAsync_ShouldCallRepositoryRemove()
        {
            var link = new LinkEntry { Id = Guid.NewGuid() };

            _linkRepositoryMock
                .Setup(r => r.RemoveAsync(link))
                .ReturnsAsync(ExecutionResponse.Successful(link.Id));

            var result = await _linkService.RemoveLinkAsync(link);

            Assert.True(result.Success);
            _linkRepositoryMock.Verify(r => r.RemoveAsync(link), Times.Once);
        }
    }
}
