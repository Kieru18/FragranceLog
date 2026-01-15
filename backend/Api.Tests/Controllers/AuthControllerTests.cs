using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    private static AuthController CreateSut(Mock<IAuthService> auth)
        => new(auth.Object);

    [Fact]
    public async Task Register_returns_ok_with_auth_response()
    {
        var auth = new Mock<IAuthService>();

        var dto = new RegisterDto
        {
            Username = "user",
            Email = "user@test.local",
            Password = "password"
        };

        var response = new AuthResponseDto
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        };

        auth.Setup(x => x.RegisterAsync(dto))
            .ReturnsAsync(response);

        var result = await CreateSut(auth).Register(dto);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        auth.Verify(x => x.RegisterAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Login_returns_ok_with_auth_response()
    {
        var auth = new Mock<IAuthService>();

        var dto = new LoginDto
        {
            UsernameOrEmail = "user@test.local",
            Password = "password"
        };

        var response = new AuthResponseDto
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        };

        auth.Setup(x => x.LoginAsync(dto))
            .ReturnsAsync(response);

        var result = await CreateSut(auth).Login(dto);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        auth.Verify(x => x.LoginAsync(dto), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_returns_ok_with_auth_response()
    {
        var auth = new Mock<IAuthService>();

        var dto = new RefreshDto
        {
            RefreshToken = "refresh-token"
        };

        var response = new AuthResponseDto
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        };

        auth.Setup(x => x.RefreshAsync(dto.RefreshToken))
            .ReturnsAsync(response);

        var result = await CreateSut(auth).RefreshToken(dto);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);

        auth.Verify(x => x.RefreshAsync(dto.RefreshToken), Times.Once);
    }

    [Fact]
    public void Register_is_allow_anonymous()
    {
        typeof(AuthController)
            .GetMethod(nameof(AuthController.Register))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
            .Should().NotBeEmpty();
    }

    [Fact]
    public void Login_is_allow_anonymous()
    {
        typeof(AuthController)
            .GetMethod(nameof(AuthController.Login))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
            .Should().NotBeEmpty();
    }

    [Fact]
    public void RefreshToken_is_allow_anonymous()
    {
        typeof(AuthController)
            .GetMethod(nameof(AuthController.RefreshToken))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
            .Should().NotBeEmpty();
    }
}
