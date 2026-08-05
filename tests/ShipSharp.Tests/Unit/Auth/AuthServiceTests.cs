using FluentAssertions;
using Moq;
using ShipSharp.Application.Auth;
using ShipSharp.Application.Auth.DTOs;
using ShipSharp.Application.Auth.Validators;
using ShipSharp.Application.Common.Exceptions;
using ShipSharp.Application.Common.Interfaces;
using ShipSharp.Domain.Users;
using Xunit;

namespace ShipSharp.Tests.Unit.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RefreshTokenRequestValidator _refreshValidator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepoMock.Object,
            _passwordServiceMock.Object,
            _tokenServiceMock.Object,
            _loginValidator,
            _refreshValidator);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAccessTokenAndRefreshToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "admin", Password = "Password123!" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashed",
            FullName = "Administrator",
            Role = UserRole.Admin
        };

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock.Setup(p => p.VerifyPassword("Password123!", "hashed"))
            .Returns(true);

        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user))
            .Returns("access_token_123");

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token_abc");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token_123");
        result.RefreshToken.Should().Be("refresh_token_abc");
        result.User.Username.Should().Be("admin");

        _userRepoMock.Verify(r => r.AddRefreshTokenAsync(It.Is<RefreshToken>(
            rt => rt.UserId == user.Id && rt.Token == "refresh_token_abc"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldRotateTokenAndReturnNewPair()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "admin", FullName = "Admin", Role = UserRole.Admin };
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid_refresh_token",
            ExpiryTime = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            User = user
        };

        var request = new RefreshTokenRequest { RefreshToken = "valid_refresh_token" };

        _userRepoMock.Setup(r => r.GetRefreshTokenAsync("valid_refresh_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user))
            .Returns("new_access_token");

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await _sut.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");

        existingToken.IsRevoked.Should().BeTrue();
        _userRepoMock.Verify(r => r.UpdateRefreshTokenAsync(existingToken, It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.AddRefreshTokenAsync(It.Is<RefreshToken>(
            rt => rt.UserId == userId && rt.Token == "new_refresh_token"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired_token",
            ExpiryTime = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            User = new User()
        };

        var request = new RefreshTokenRequest { RefreshToken = "expired_token" };

        _userRepoMock.Setup(r => r.GetRefreshTokenAsync("expired_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        // Act & Assert
        var act = async () => await _sut.RefreshTokenAsync(request);
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*expired refresh token*");
    }
}
