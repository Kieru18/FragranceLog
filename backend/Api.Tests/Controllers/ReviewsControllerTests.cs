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

public sealed class ReviewsControllerTests
{
    private static ReviewsController CreateSut(
        Mock<IReviewService> service,
        ClaimsPrincipal? user = null)
    {
        var controller = new ReviewsController(service.Object);

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
    public async Task SaveReview_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IReviewService>();
        var sut = CreateSut(service);

        var result = await sut.SaveReview(new SaveReviewDto());

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveReview_calls_service_and_returns_no_content()
    {
        var service = new Mock<IReviewService>();
        var sut = CreateSut(service, User());

        var dto = new SaveReviewDto();

        var result = await sut.SaveReview(dto);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.CreateOrUpdateAsync(123, dto),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserReview_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IReviewService>();
        var sut = CreateSut(service);

        var result = await sut.GetCurrentUserReview(1);

        result.Result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCurrentUserReview_returns_not_found_when_no_review()
    {
        var service = new Mock<IReviewService>();
        service.Setup(x =>
                x.GetByUserAndPerfumeAsync(123, 1))
            .ReturnsAsync((ReviewDto?)null);

        var sut = CreateSut(service, User());

        var result = await sut.GetCurrentUserReview(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCurrentUserReview_returns_ok_when_review_exists()
    {
        var service = new Mock<IReviewService>();
        var review = new ReviewDto();

        service.Setup(x =>
                x.GetByUserAndPerfumeAsync(123, 1))
            .ReturnsAsync(review);

        var sut = CreateSut(service, User());

        var result = await sut.GetCurrentUserReview(1);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(review);
    }

    [Fact]
    public async Task Delete_returns_unauthorized_when_no_user()
    {
        var service = new Mock<IReviewService>();
        var sut = CreateSut(service);

        var result = await sut.Delete(1, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_calls_service_and_returns_no_content()
    {
        var service = new Mock<IReviewService>();
        var sut = CreateSut(service, User());

        var result = await sut.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        service.Verify(x =>
            x.DeleteAsync(1, 123, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
