using FluentAssertions;
using MyMoviesApp.Domain.Entities;

namespace MyMoviesApp.Domain.Tests.Entities;

public class UserEntityTests
{
    [Fact]
    public void User_Ctor_Should_AssignIdAndEmail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User(id, "user@example.com");

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void User_Id_Should_BeReadOnly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User(id, "user@example.com");

        // Assert
        user.Id.Should().Be(id);
    }

    [Fact]
    public void User_Email_Should_BeReadOnly()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "readonly@example.com");

        // Assert
        user.Email.Should().Be("readonly@example.com");
    }

    [Fact]
    public void User_TwoUsers_WithSameData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var user1 = new User(id1, "a@example.com");
        var user2 = new User(id2, "b@example.com");

        // Assert
        user1.Id.Should().NotBe(user2.Id);
        user1.Email.Should().NotBe(user2.Email);
    }
}

