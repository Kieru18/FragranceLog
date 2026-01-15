using Api.Controllers;
using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class PerfumeVotesControllerTests
{
    private static PerfumeVotesController CreateSut(
        Mock<IPerfumeVoteService> service,
        ClaimsPrincipal? user = null)
    {
        var controller = new PerfumeVotesController(service.Object);

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
    public async Task SetGenderVote_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service);

        var result = await sut.SetGenderVote(
            1,
            new SetGenderVoteDto { Gender = GenderEnum.Male });

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetGenderVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.SetGenderVote(
            10,
            new SetGenderVoteDto { Gender = GenderEnum.Male });

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.SetGenderVoteAsync(10, 123, GenderEnum.Male),
            Times.Once);
    }

    [Fact]
    public async Task SetSillageVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.SetSillageVote(
            5,
            new SetSillageVoteDto { Sillage = SillageEnum.Strong });

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.SetSillageVoteAsync(5, 123, SillageEnum.Strong),
            Times.Once);
    }

    [Fact]
    public async Task SetLongevityVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.SetLongevityVote(
            7,
            new SetLongevityVoteDto { Longevity = LongevityEnum.LongLasting });

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.SetLongevityVoteAsync(7, 123, LongevityEnum.LongLasting),
            Times.Once);
    }

    [Fact]
    public async Task SetSeasonVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.SetSeasonVote(
            9,
            new SetSeasonVoteDto { Season = SeasonEnum.Winter });

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.SetSeasonVoteAsync(9, 123, SeasonEnum.Winter),
            Times.Once);
    }

    [Fact]
    public async Task SetDaytimeVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.SetDaytimeVote(
            3,
            new SetDaytimeVoteDto { Daytime = DaytimeEnum.Night });

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.SetDaytimeVoteAsync(3, 123, DaytimeEnum.Night),
            Times.Once);
    }

    [Fact]
    public async Task DeleteGenderVote_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service);

        var result = await sut.DeleteGenderVote(1, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteGenderVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.DeleteGenderVote(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteGenderVoteAsync(1, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteLongevityVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.DeleteLongevityVote(2, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteLongevityVoteAsync(2, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteSillageVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.DeleteSillageVote(3, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteSillageVoteAsync(3, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteSeasonVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.DeleteSeasonVote(4, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteSeasonVoteAsync(4, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteDaytimeVote_calls_service_and_returns_no_content()
    {
        var service = new Mock<IPerfumeVoteService>();
        var sut = CreateSut(service, User());

        var result = await sut.DeleteDaytimeVote(5, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteDaytimeVoteAsync(5, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
