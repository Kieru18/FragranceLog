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

public sealed class HomeControllerTests
{
    private static HomeController CreateSut(
        Mock<IPerfumeAnalyticsService> analytics,
        Mock<IHomeInsightService> insights,
        ClaimsPrincipal? user = null)
    {
        var controller = new HomeController(
            analytics.Object,
            insights.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user ?? new ClaimsPrincipal()
            }
        };

        return controller;
    }

    private static ClaimsPrincipal CreateUser(int userId)
        => new(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                },
                "test"));

    [Fact]
    public async Task GetPerfumeOfTheDay_returns_204_when_service_returns_null()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        analytics
            .Setup(x => x.GetPerfumeOfTheDayAsync())
            .ReturnsAsync((PerfumeOfTheDayDto?)null);

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetPerfumeOfTheDay();

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetPerfumeOfTheDay_returns_200_when_service_returns_dto()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        var dto = new PerfumeOfTheDayDto
        {
            PerfumeId = 1,
            Name = "Test"
        };

        analytics
            .Setup(x => x.GetPerfumeOfTheDayAsync())
            .ReturnsAsync(dto);

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetPerfumeOfTheDay();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetRecentReviews_returns_200_and_forwards_take()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        analytics
            .Setup(x => x.GetRecentReviewsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HomeRecentReviewDto>());

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetRecentReviews(5);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<HomeRecentReviewDto>>();

        analytics.Verify(
            x => x.GetRecentReviewsAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStats_returns_200()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        var dto = new HomeStatsDto
        {
            Perfumes = 10,
            Users = 5,
            Reviews = 20
        };

        analytics
            .Setup(x => x.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetStats();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetInsights_returns_401_when_user_not_authenticated()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetInsights(default);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetInsights_returns_204_when_no_insights()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        insights
            .Setup(x => x.GetInsightsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HomeInsightDto>());

        var sut = CreateSut(
            analytics,
            insights,
            CreateUser(1));

        var result = await sut.GetInsights(default);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetInsights_returns_200_when_insights_exist()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        var data = new[]
        {
            new HomeInsightDto
            {
                Key = "test",
                Title = "Test",
                Subtitle = "Test",
                Scope = Core.Enums.InsightScopeEnum.Global
            }
        };

        insights
            .Setup(x => x.GetInsightsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var sut = CreateSut(
            analytics,
            insights,
            CreateUser(1));

        var result = await sut.GetInsights(default);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task GetTopFromCountry_returns_200_and_uses_take_3()
    {
        var analytics = new Mock<IPerfumeAnalyticsService>();
        var insights = new Mock<IHomeInsightService>();

        analytics
            .Setup(x =>
                x.GetTopFromCountryAsync(
                    50,
                    20,
                    3,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HomeCountryPerfumeDto>());

        var sut = CreateSut(analytics, insights);

        var result = await sut.GetTopFromCountry(50, 20, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        analytics.Verify(
            x => x.GetTopFromCountryAsync(
                50,
                20,
                3,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
