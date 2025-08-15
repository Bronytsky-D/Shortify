using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shortify.Domain.Models;
using Shottify.Application.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortify.Application.Test.Service
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var storeMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                storeMock.Object, null, null, null, null, null, null, null, null
            );

            var users = new List<User>
        {
            new User { Id = "user1", Email = "user1@example.com" },
            new User { Id = "user2", Email = "user2@example.com" }
        };

            _userManagerMock
                .Setup(um => um.Users)
                .Returns(users.AsQueryable());

            _userService = new UserService(_userManagerMock.Object);
        }
        [Fact]
        public async Task CreateUserAsync_ShouldReturnUser_WhenSucceeded()
        {
            var newUser = new User { Id = "newUser", Email = "new@example.com" };
            _userManagerMock.Setup(um => um.CreateAsync(newUser, "password"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userService.CreateUserAsync(newUser, "password");

            Assert.True(result.Success);
            Assert.Equal(newUser, result.Result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldFail_WhenIdentityFails()
        {
            var newUser = new User { Id = "newUser", Email = "new@example.com" };
            _userManagerMock.Setup(um => um.CreateAsync(newUser, "password"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            var result = await _userService.CreateUserAsync(newUser, "password");

            Assert.False(result.Success);
            Assert.Equal("Error", result.Errors.First());
        }

        [Fact]
        public async Task AddToRoleAsync_ShouldReturnUser_WhenSucceeded()
        {
            var user = new User { Id = "user1" };
            _userManagerMock.Setup(um => um.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userService.AddToRoleAsync(user, "Admin");

            Assert.True(result.Success);
            Assert.Equal(user, result.Result);
        }

        [Fact]
        public async Task GetRolesAsync_ShouldReturnRoles_WhenExists()
        {
            var user = new User { Id = "user1" };
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            var result = await _userService.GetRolesAsync(user);

            Assert.True(result.Success);
            var roles = Assert.IsAssignableFrom<IEnumerable<string>>(result.Result);
            Assert.Contains("Admin", roles);
        }

        [Fact]
        public async Task CheckUserPassword_ShouldReturnTrue_WhenCorrect()
        {
            var user = new User { Id = "user1" };
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, "pass"))
                .ReturnsAsync(true);

            var result = await _userService.CheckUserPassword(user, "pass");

            Assert.True(result);
        }
    }
}
