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

public sealed class PerfumesControllerTests
{
    private static PerfumesController CreateSut(
        Mock<IPerfumeService> perfumeService,
        int? userId = null)
    {
        var controller = new PerfumesController(perfumeService.Object);

        if (userId != null)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())
                            },
                            "test"))
                }
            };
        }

        return controller;
    }

    [Fact]
    public async Task Search_returns_ok_and_delegates_to_service()
    {
        var service = new Mock<IPerfumeService>();

        var response = new PerfumeSearchResponseDto
        {
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            Items = Array.Empty<PerfumeSearchResultDto>()
        };

        service
            .Setup(x => x.SearchAsync(
                It.IsAny<PerfumeSearchRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = CreateSut(service);

        var req = new PerfumeSearchRequestDto
        {
            Page = 1,
            PageSize = 10
        };

        var result = await sut.Search(req, default);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);

        service.Verify(
            x => x.SearchAsync(req, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_returns_unauthorized_when_user_not_authenticated()
    {
        var service = new Mock<IPerfumeService>();
        var sut = CreateSut(service);

        var result = await sut.GetById(1, default);

        result.Result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_returns_ok_when_user_authenticated()
    {
        var service = new Mock<IPerfumeService>();

        var details = new PerfumeDetailsDto
        {
            PerfumeId = 1,
            Name = "Test Perfume"
        };

        service
            .Setup(x => x.GetDetailsAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);

        var sut = CreateSut(service, userId: 10);

        var result = await sut.GetById(1, default);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(details);

        service.Verify(
            x => x.GetDetailsAsync(
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
