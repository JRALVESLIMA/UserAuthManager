using System.Threading.Tasks;
using Moq;
using UserAuthManager.API.Models;
using UserAuthManager.API.Repositories;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace UserAuthManager.Tests
{
    public class UserRepositoryTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserRepositoryTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnListOfUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "User 1", Email = "user1@example.com", UserName = "user1" },
                new ApplicationUser { Id = "2", Name = "User 2", Email = "user2@example.com", UserName = "user2" }
            };

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _userRepositoryMock.Object.GetAllUsersAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("User 1");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = new ApplicationUser
            {
                Id = "1",
                Name = "Teste Usuário",
                Email = "Teste@example.com",
                UserName = "testUser"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync("1")).ReturnsAsync(user);

            var result = await _userRepositoryMock.Object.GetUserByIdAsync("1");

            result.Should().NotBeNull();
            result.Id.Should().Be("1");
            result.Name.Should().Be("Teste Usuário");
        }

        [Fact]
        public async Task AddUserAsync_ShouldReturnUser_WhenUserIsAddedSuccessfully()
        {
            var user = new ApplicationUser
            {
                Id = "2",
                Name = "Novo Usuário",
                Email = "novo@example.com",
                UserName = "novoUser"
            };

            _userRepositoryMock.Setup(repo => repo.AddUserAsync(user)).ReturnsAsync(user);

            var result = await _userRepositoryMock.Object.AddUserAsync(user);

            result.Should().NotBeNull();
            result.Id.Should().Be("2");
            result.Name.Should().Be("Novo Usuário");
            result.Email.Should().Be("novo@example.com");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser_WhenUserExists()
        {
            var updatedUser = new ApplicationUser
            {
                Id = "3",
                Name = "Atualizado Usuário",
                Email = "atualizado@example.com",
                UserName = "novoUser"
            };

            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<ApplicationUser>()))
                               .ReturnsAsync(updatedUser);

            var result = await _userRepositoryMock.Object.UpdateUserAsync(updatedUser);

            result.Should().NotBeNull();
            result.Id.Should().Be("3");
            result.Name.Should().Be("Atualizado Usuário");
            result.Email.Should().Be("atualizado@example.com");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
        {
            string userId = "1";

            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(userId))
                               .ReturnsAsync(true);

            var result = await _userRepositoryMock.Object.DeleteUserAsync(userId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            string invalidUserId = "999";

            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(invalidUserId))
                               .ReturnsAsync(false);

            var result = await _userRepositoryMock.Object.DeleteUserAsync(invalidUserId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            string userId = "99";

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                               .ReturnsAsync((ApplicationUser?)null);

            var result = await _userRepositoryMock.Object.GetUserByIdAsync(userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers_Correctly()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "Usuário 1", Email = "user1@example.com", UserName = "user1" },
                new ApplicationUser { Id = "2", Name = "Usuário 2", Email = "user2@example.com", UserName = "user2" }
            };

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _userRepositoryMock.Object.GetAllUsersAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.ElementAt(0).Name.Should().Be("Usuário 1");
            result.ElementAt(1).Email.Should().Be("user2@example.com");
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            var email = "teste@email.com";
            var user = new ApplicationUser
            {
                Id = "5",
                Name = "Usuário Teste",
                Email = email,
                UserName = "testuser"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                               .ReturnsAsync(user);

            var result = await _userRepositoryMock.Object.GetUserByEmailAsync(email);

            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Name.Should().Be("Usuário Teste");
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            var email = "inexistente@email.com";

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                               .ReturnsAsync((ApplicationUser?)null);

            var result = await _userRepositoryMock.Object.GetUserByEmailAsync(email);

            result.Should().BeNull();
        }


    }
}
