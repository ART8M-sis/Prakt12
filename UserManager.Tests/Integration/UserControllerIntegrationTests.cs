using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using UserManager.DTO;

namespace UserManager.Tests.Integration
{
    public class UserControllerIntegrationTests : IClassFixture<UserManagerWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly UserManagerWebApplicationFactory _factory;

        public UserControllerIntegrationTests(UserManagerWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
            _factory.UserService.Reset();
        }

        [Fact]
        public async Task GetAll_WhenUsersExist_Returns200OK_WithUsersList()
        {
            var users = new List<UserDTO>
            {
                new UserDTO
                {
                    Id = Guid.NewGuid(),
                    Email = "1test@test.com",
                    FirstName = "1Name",
                    LastName = "1LastName",
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                },
                new UserDTO
                {
                    Id = Guid.NewGuid(),
                    Email = "2test@test.com",
                    FirstName = "2Name",
                    LastName = "2LastName",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _factory.UserService.Setup(ser => ser.GetUsersAsync()).ReturnsAsync(users);

            var response = await _client.GetAsync("api/user");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(users[0].Email, result[0].Email);
            Assert.Equal(users[1].Email, result[1].Email);

            _factory.UserService.Verify(ser => ser.GetUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenNoUsers_Returns200OK_WithEmptyList()
        {
            _factory.UserService.Setup(ser => ser.GetUsersAsync()).ReturnsAsync(new List<UserDTO>());

            var response = await _client.GetAsync("api/user");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
            Assert.NotNull(result);
            Assert.Empty(result);

            _factory.UserService.Verify(ser => ser.GetUsersAsync(), Times.Once);
        }

        [Fact] //////////////////////////////////////////////////
        public async Task GetById_WhenUserExists_Returns200OK_WithUser()
        {
            var userId = Guid.NewGuid();
            var expectedUser = new UserDTO
            {
                Id = userId,
                FirstName = "Test",
                Email = "test@test.com",
                LastName = "Test"
            };
            _factory.UserService.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(expectedUser);
            var response = await _client.GetAsync($"api/user/get/{userId}");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var resultDto = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(resultDto);
            Assert.Equal(userId, resultDto.Id);
            Assert.Equal("test@test.com", resultDto.Email);
            _factory.UserService.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenUserDoesNotExist_Returns404NotFound()
        {
            var userId = Guid.NewGuid();
            _factory.UserService.Setup(ser => ser.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO?)null);

            var response = await _client.GetAsync($"api/user/get/{userId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _factory.UserService.Verify(ser => ser.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenIdIsNotGuid_Returns404NotFound()
        {
            var response = await _client.GetAsync("api/user/get/not-a-guid");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenDtoIsValid_Returns201Created()
        {
            var newUserId = Guid.NewGuid();
            var createDto = new CreateUserDTO
            {
                Email = "new@test.com",
                FirstName = "New",
                LastName = "User",
                Role = "User"
            };

            _factory.UserService
                .Setup(ser => ser.CreateUserAsync(It.IsAny<CreateUserDTO>()))
                .ReturnsAsync(new UserDTO { Id = newUserId, Email = createDto.Email });

            var response = await _client.PostAsJsonAsync("api/user", createDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.Equal(newUserId, result!.Id);

            _factory.UserService.Verify(
                ser => ser.CreateUserAsync(It.Is<CreateUserDTO>(d => d.Email == "new@test.com")),
                Times.Once);
        }

        [Fact]
        public async Task Create_PassesCorrectDtoToService()
        {
            var createDto = new CreateUserDTO
            {
                Email = "check@test.com",
                FirstName = "First",
                LastName = "Last",
                Role = "Admin"
            };

            _factory.UserService.Setup(ser => ser.CreateUserAsync(It.IsAny<CreateUserDTO>())).ReturnsAsync(new UserDTO { Id = Guid.NewGuid() });

            await _client.PostAsJsonAsync("api/user", createDto);

            _factory.UserService.Verify(
                ser => ser.CreateUserAsync(It.Is<CreateUserDTO>(d =>
                    d.Email == "check@test.com" &&
                    d.FirstName == "First" &&
                    d.LastName == "Last" &&
                    d.Role == "Admin")),
                Times.Once);
        }

        [Fact]
        public async Task Delete_WhenUserExists_Returns204NoContent()
        {
            var userId = Guid.NewGuid();
            _factory.UserService.Setup(ser => ser.DeleteUserAsync(userId)).ReturnsAsync(true);

            var response = await _client.DeleteAsync($"api/user/{userId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            _factory.UserService.Verify(ser => ser.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenUserDoesNotExist_Returns404NotFound()
        {
            var userId = Guid.NewGuid();
            _factory.UserService.Setup(ser => ser.DeleteUserAsync(userId)).ReturnsAsync(false);

            var response = await _client.DeleteAsync($"api/user/{userId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _factory.UserService.Verify(ser => ser.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Delete_PassesCorrectIdToService()
        {
            var userId = Guid.NewGuid();
            _factory.UserService.Setup(ser => ser.DeleteUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            await _client.DeleteAsync($"api/user/{userId}");

            _factory.UserService.Verify(ser => ser.DeleteUserAsync(userId), Times.Once);
        }
    }
}
