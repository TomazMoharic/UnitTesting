using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Users.Api.Logging;
using Users.Api.Models;
using Users.Api.Repositories;
using Users.Api.Services;
using Xunit;

namespace Users.Api.Tests.Unit.Application;

public class UserServiceTests
{
    private readonly UserService _sut;
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ILoggerAdapter<UserService> _logger = Substitute.For<ILoggerAdapter<UserService>>();

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _logger);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers_WhenSomeUsersExist()
    {
        // Arrange
        var nickChapsas = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nick Chapsas"
        };
        var expectedUsers = new[]
        {
            nickChapsas
        };
        _userRepository.GetAllAsync().Returns(expectedUsers);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        //result.Single().Should().BeEquivalentTo(nickChapsas);
        result.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetAllAsync_ShouldLogMessages_WhenInvoked()
    {
        // Arrange
        _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>());

        // Act
        await _sut.GetAllAsync();

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Retrieving all users"));
        _logger.Received(1).LogInformation(Arg.Is("All users retrieved in {0}ms"), Arg.Any<long>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
    {
        // Arrange
        var sqliteException = new SqliteException("Something went wrong", 500);
        _userRepository.GetAllAsync()
            .Throws(sqliteException);

        // Act
        var requestAction = async () => await _sut.GetAllAsync();

        // Assert
        await requestAction.Should()
            .ThrowAsync<SqliteException>().WithMessage("Something went wrong");
        _logger.Received(1).LogError(Arg.Is(sqliteException), Arg.Is("Something went wrong while retrieving all users"));
    }
    
    //Authored code
    [Fact]
    public async Task GetByIdAsync_ShouldReturnAUser_WhenAUserExists()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        User user = new()
        {
            Id = guid,
            FullName = "Funny Guy"
        };

        _userRepository.GetByIdAsync(guid).Returns(user);
        
        // Act
       User? result = await _sut.GetByIdAsync(guid);

        // Assert
        result.Should().BeEquivalentTo(user);
    }
    
    //Authored code
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist ()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        _userRepository.GetByIdAsync(guid).ReturnsNull();

        // Act
        User? result = await _sut.GetByIdAsync(guid);

        // Assert
        result.Should().BeNull();
    } 
    
    //Authored code
    [Fact]
    public async Task GetByIdAsync_ShouldLogCorrectMessages_WhenInvoked()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        _userRepository.GetByIdAsync(guid).Returns(new User());
        
        // Act
        await _sut.GetByIdAsync(guid);

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Retrieving user with id: {0}"), Arg.Is(guid));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} retrieved in {1}ms"), Arg.Is(guid), Arg.Any<long>());
    }
    
    //Authored code
    [Fact]
    public async Task GetByIdAsync_ShouldLogCorrectMessageAndException_WhenExceptionIsThrown()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        
        Exception exception = new("Something terrible happened!");

        _userRepository.GetByIdAsync(guid).Throws(exception);
        
        // Act
        var action = async () => await _sut.GetByIdAsync(guid);

        // Assert
        await action.Should()
            .ThrowAsync<Exception>().WithMessage("Something terrible happened!");
        
        _logger.Received(1)
            .LogError(Arg.Is(exception), Arg.Is("Something went wrong while retrieving user with id {0}"), Arg.Is(guid));
    }
    
    //Authored code
    [Fact]
    public async Task CreateAsync_ShouldCreateAUser_WhenUserCreateDetailsAreValid()
    {
        // check this against the solution, it feels wrong
        
        // Arrange
        User userModel = new()
        {
            FullName = "Funny Guy",
        };

        _userRepository.CreateAsync(userModel).Returns(true);
        
        // Act
        bool response = await _sut.CreateAsync(userModel);
        
        // Assert
        // response.Should().Be(true);
        response.Should().BeTrue();
    }
    
    //Authored code
    [Fact]
    public async Task CreatAsync_ShouldLogTheCorrectMessage_WhenCreatingAUser()
    {
        // Arrange
        User userModel = new()
        {
            FullName = "Funny Guy",
        };

        _userRepository.CreateAsync(userModel).Returns(new bool());
        
        // Act
        await _sut.CreateAsync(userModel);
        
        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Creating user with id {0} and name: {1}"), Arg.Is(userModel.Id), Arg.Is(userModel.FullName));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} created in {1}ms"), Arg.Is(userModel.Id), Arg.Any<long>());
    }
    
    //Authored code
    [Fact]
    public async Task CreateAsync_ShouldLogTheCorrectMessage_WhenAnExceptionIsThrown()
    {
        // Arrange
        User userModel = new()
        {
            FullName = "Funny Guy",
        };
        
        Exception exception = new("Something terrible happened!");
        
        _userRepository.CreateAsync(userModel).Throws(exception);
        
        // Act
        var action = async () => await _sut.CreateAsync(userModel);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Something terrible happened!");
        
        _logger.Received(1).LogError(Arg.Is(exception), Arg.Is("Something went wrong while creating a user"));
    }
    
    //Authored code
    [Fact]
    public async Task DeleteByIdAsync_ShouldDeleteAUser_WhenTheUserExists()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        _userRepository.DeleteByIdAsync(guid).Returns(true);

        // Act
        bool result = await _sut.DeleteByIdAsync(guid);
        
        // Assert
        // result.Should().Be(true);
        result.Should().BeTrue();
    }
    
    //Authored code
    [Fact]
    public async Task DeleteByIdAsync_ShouldNotDeleteAUser_WhenTheUserDoesNotExist()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        _userRepository.DeleteByIdAsync(guid).Returns(false);

        // Act
        bool result = await _sut.DeleteByIdAsync(guid);
        
        // Assert
        // result.Should().Be(false);
        result.Should().BeFalse();
    }
    
    //Authored code
    [Fact]
    public async Task DeleteByIdAsync_ShouldLogTheCorrectMessage_WhenDeletingAUser()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        _userRepository.DeleteByIdAsync(guid).Returns(new bool());

        // Act
        await _sut.DeleteByIdAsync(guid);
        
        // Assert
        _logger.Received(1).LogInformation("Deleting user with id: {0}", Arg.Is(guid));
        _logger.Received(1).LogInformation("User with id {0} deleted in {1}ms", Arg.Is(guid), Arg.Any<long>());
        
    }
    
    //Authored code
    [Fact]
    public async Task DeleteByIdAsync_ShouldLogACorrectMessage_WhenExceptionIsThrown()
    {
        // Arrange  
        Guid guid = Guid.NewGuid();

        Exception exception = new("Something awful happened.");
        
        _userRepository.DeleteByIdAsync(guid).Throws(exception);
        
        // Act
        var action = async () => await _sut.DeleteByIdAsync(guid);
        
        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Something awful happened.");
        
        _logger.Received(1).LogError(exception, "Something went wrong while deleting user with id {0}", Arg.Is(guid));
    }
}
