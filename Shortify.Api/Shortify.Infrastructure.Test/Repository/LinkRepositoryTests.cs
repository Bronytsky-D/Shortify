using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;
using Shortify.Infrastructure.PostgreSQL.ApplicationContext;
using Shortify.Infrastructure.PostgreSQL.Repository;
using Shortify.Infrastructure.Test.Utils;



namespace Shortify.Infrastructure.Test.Repository
{
    public class LinkRepositoryTests
    {

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }
        [Fact]
        public async Task AddLinkAsync_ShouldAddLink()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);
            var linkEntry = LinkFactory.CreateLink("user123");
            // Act
            var result = await repository.AddAsync(linkEntry);
            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetLinksAsync_ShouldReturnAllLinks()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);
            await repository.AddAsync(LinkFactory.CreateLink("user123"));
            await repository.AddAsync(LinkFactory.CreateLink("user122"));
            
            // Act
            var result = await repository.GetLinksAsync();
            
            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty((IEnumerable<LinkEntry>)result.Result);
        }
        [Fact]
        public async Task GetLinksAsync_WhenNoLinks_ShouldReturnFailure()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);

            var result = await repository.GetLinksAsync();

            Assert.False(result.Success);
            Assert.Equal("No links found.", result.Errors.First());
        }

        [Fact]
        public async Task GetLinkAsync_ShouldReturnLinkById()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);
            var linkEntry = LinkFactory.CreateLink("user123");

            var addResult = await repository.AddAsync(linkEntry);
            var addedLink = (LinkEntry)addResult.Result; 

            var result = await repository.GetLinkAsync(addedLink.Id);

            Assert.True(result.Success);
            Assert.NotNull(result.Result);
        }
        [Fact]
        public async Task GetLinkAsync_WhenNotFound_ShouldReturnFailure()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);

            var result = await repository.GetLinkAsync(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("No links found.", result.Errors.First());
        }
        [Fact]
        public async Task GetByUserAsync_ShouldReturnLinksForSpecificUser()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);

            await repository.AddAsync(LinkFactory.CreateLink("user123"));
            await repository.AddAsync(LinkFactory.CreateLink("user122"));

            var result = await repository.GetByUserAsync("user123");

            Assert.True(result.Success);
            var links = Assert.IsAssignableFrom<IEnumerable<LinkEntry>>(result.Result);
            Assert.Single(links);
            Assert.Equal("user123", links.First().CreatedBy);
        }
        [Fact]
        public async Task UpdateAsync_ShouldUpdateLink()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);

            var addResult = await repository.AddAsync(LinkFactory.CreateLink("user123"));
            var addedLink = (LinkEntry)addResult.Result;

            addedLink.OriginalUrl = "https://new.com";
            var updateResult = await repository.UpdateAsync(addedLink);

            Assert.True(updateResult.Success);
            var updated = await repository.GetLinkAsync(addedLink.Id);
            Assert.Equal("https://new.com", ((LinkEntry)updated.Result).OriginalUrl);
        }

        [Fact]
        public async Task RemoveLinkAsync_ShouldRemoveLink()
        {
            var context = GetInMemoryDbContext();
            var repository = new LinkRepository(context);
            var linkEntry = LinkFactory.CreateLink("user123");

            var addResult = await repository.AddAsync(linkEntry);
            var addedLink = (LinkEntry)addResult.Result; 

            var removeResult = await repository.RemoveAsync(addedLink);

            Assert.True(removeResult.Success);

            var getResult = await repository.GetLinkAsync(addedLink.Id);
            Assert.False(getResult.Success);
        }
    }
}
