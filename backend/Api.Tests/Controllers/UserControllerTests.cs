using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class UserControllerTests
{
    private static UserController CreateSut(
        Mock<IUserService> service,
        ClaimsPrincipal? user = null)
    {
        var controller = new UserController(service.Object);

        if (user != null)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        return controller;
    }

    private static ClaimsPrincipal User(int id = 123)
        => new(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) },
            "test"));

    [Fact]
    public async Task Me_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IUserService>();
        var sut = CreateSut(service);

        var result = await sut.Me();

        result.Result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Me_calls_service_and_returns_ok()
    {
        var service = new Mock<IUserService>();
        var profile = new UserProfileDto();

        service.Setup(x => x.GetMeAsync(123))
            .ReturnsAsync(profile);

        var sut = CreateSut(service, User());

        var result = await sut.Me();

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(profile);
    }

    [Fact]
    public async Task UpdateProfile_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IUserService>();
        var sut = CreateSut(service);

        var result = await sut.UpdateProfile(new UpdateProfileDto());

        result.Result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateProfile_calls_service_and_returns_ok()
    {
        var service = new Mock<IUserService>();
        var dto = new UpdateProfileDto();
        var profile = new UserProfileDto();

        service.Setup(x => x.UpdateProfileAsync(123, dto))
            .ReturnsAsync(profile);

        var sut = CreateSut(service, User());

        var result = await sut.UpdateProfile(dto);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(profile);
    }

    [Fact]
    public async Task ChangePassword_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IUserService>();
        var sut = CreateSut(service);

        var result = await sut.ChangePassword(new ChangePasswordDto());

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_calls_service_and_returns_no_content()
    {
        var service = new Mock<IUserService>();
        var dto = new ChangePasswordDto();

        var sut = CreateSut(service, User());

        var result = await sut.ChangePassword(dto);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.ChangePasswordAsync(123, dto),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IUserService>();
        var sut = CreateSut(service);

        var result = await sut.DeleteAccount();

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteAccount_calls_service_and_returns_no_content()
    {
        var service = new Mock<IUserService>();

        var sut = CreateSut(service, User());

        var result = await sut.DeleteAccount();

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteAccountAsync(123),
            Times.Once);
    }
}
