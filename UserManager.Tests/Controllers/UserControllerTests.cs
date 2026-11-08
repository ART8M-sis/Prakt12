using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using UserManager.Controllers;
using UserManager.DTO;
using UserManager.Models;
using UserManager.Services;

namespace UserManager.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _usersController;
        private readonly Mock<IUserService> _userServiceMock;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();

            _usersController = new UserController(
                _userServiceMock.Object
                );
        }

        [Fact]
        public async Task GetAll_WhenUsersExist_ReturnsOkObjectResult()
        {   
            //Arrange
            var users = new List<UserDTO>
            {
                new UserDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Email = "1test@test.com", 
                    FirstName = "1Name", 
                    LastName = "1LastName" 
                },
                new UserDTO 
                { Id = Guid.NewGuid(), 
                    Email = "2test@test.com", 
                    FirstName = "2Name", 
                    LastName = "2LastName" 
                }
            };

            _userServiceMock.Setup(ser => ser.GetUsersAsync()).ReturnsAsync(users);

            //Act
            var result = await _usersController.GetAll();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task GetAll_WhenNoUsers_ReturnsOkObjectResultWithEmptyList()
        {
            //Arrange
            var users = new List<UserDTO> { };
            _userServiceMock.Setup(ser => ser.GetUsersAsync()).ReturnsAsync(users);

            //Act
            var result = await _usersController.GetAll();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task GetById_WhenUserExist_ReturnsOkObjectResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var user = new UserDTO 
            {
                Id = userId, 
                Email = "test@test.com", 
                FirstName = "Name",
                LastName = "LastName" 
            };

            _userServiceMock.Setup(ser => ser.GetUserByIdAsync(userId)).ReturnsAsync(user);

            //Act
            var result = await _usersController.GetById(userId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenUserDoesNotExist_ReturnsNotFoundResult()
        {
            //Arrange
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(ser => ser.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO?)null);

            //Act
            var result = await _usersController.GetById(userId);

            //Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_WhenDtoIsValid_ReturnsCreatedAtActionResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var createDto = new CreateUserDTO
            {
                Email = "test@test.com",
                FirstName = "Name",
                LastName = "LastName",
                Role = "User"
            };

            var createdUser = new UserDTO
            {
                Id = userId,
                Email = createDto.Email,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Role = createDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _userServiceMock.Setup(ser => ser.CreateUserAsync(createDto)).ReturnsAsync(createdUser);

            //Act
            var result = await _usersController.Create(createDto);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(createdUser, createdResult.Value);
        }

        [Fact]
        public async Task Create_CallsServiceOnceWithCorrectDto()
        {
            //Arrange
            var createDto = new CreateUserDTO 
            {
                Email = "test@test.com",
                FirstName = "Name",
                LastName = "LastName",
                Role = "User"
            };


            _userServiceMock.Setup(ser => ser.CreateUserAsync(createDto)).ReturnsAsync(new UserDTO { Id = Guid.NewGuid() });

            //Act
            await _usersController.Create(createDto);

            //Assert
            _userServiceMock.Verify(
                ser => ser.CreateUserAsync(It.Is<CreateUserDTO>(d => d.Email == "test@test.com")),Times.Once);
        }






        [Fact]
        public async Task Delete_WhenUserExists_ReturnsNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(ser => ser.DeleteUserAsync(userId)) .ReturnsAsync(true);

            // Act
            var result = await _usersController.Delete(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(ser => ser.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _usersController.Delete(userId);

            // Assert: очікуємо 404 NotFound
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_CallsServiceWithCorrectId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(ser => ser.DeleteUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _usersController.Delete(userId);

            // Assert
            _userServiceMock.Verify(
                ser => ser.DeleteUserAsync(userId),
                Times.Once);
        }
    }
}