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

public sealed class SharedListsControllerTests
{
    private static SharedListsController CreateSut(
        Mock<ISharedListService> service,
        ClaimsPrincipal? user = null)
    {
        var controller = new SharedListsController(service.Object);

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
    public async Task GetPreview_returns_not_found_when_missing()
    {
        var service = new Mock<ISharedListService>();
        var token = Guid.NewGuid();

        service.Setup(x => x.GetPreviewAsync(token))
            .Returns(Task.FromResult<SharedListPreviewDto>(null!));

        var sut = CreateSut(service);

        var result = await sut.GetPreview(token);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPreview_returns_ok_when_exists()
    {
        var service = new Mock<ISharedListService>();
        var token = Guid.NewGuid();
        var preview = new SharedListPreviewDto();

        service.Setup(x => x.GetPreviewAsync(token))
            .ReturnsAsync(preview);

        var sut = CreateSut(service);

        var result = await sut.GetPreview(token);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(preview);
    }

    [Fact]
    public async Task Import_returns_unauthorized_when_no_user()
    {
        var service = new Mock<ISharedListService>();
        var sut = CreateSut(service);
        var token = Guid.NewGuid();

        var result = await sut.Import(token);

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Import_calls_service_and_returns_ok()
    {
        var service = new Mock<ISharedListService>();
        var token = Guid.NewGuid();

        service.Setup(x => x.ImportAsync(123, token))
            .ReturnsAsync(456);

        var sut = CreateSut(service, User());

        var result = await sut.Import(token);

        result.Should().BeOfType<OkObjectResult>();

        service.Verify(x =>
            x.ImportAsync(123, token),
            Times.Once);
    }
}
