using Api.Controllers;
using Core.Dtos;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class PerfumeListsControllerTests
{
    private static PerfumeListsController CreateSut(
        Mock<IPerfumeListService> lists,
        Mock<ISharedListService> shared,
        ClaimsPrincipal? user = null)
    {
        var controller = new PerfumeListsController(
            lists.Object,
            shared.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user ?? new ClaimsPrincipal()
            }
        };

        return controller;
    }

    private static ClaimsPrincipal User(int id)
        => new(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) },
                "test"));

    [Fact]
    public async Task GetLists_returns_401_when_not_authenticated()
    {
        var sut = CreateSut(
            new Mock<IPerfumeListService>(),
            new Mock<ISharedListService>());

        var result = await sut.GetLists();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetLists_returns_200_with_lists()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.GetUserListsAsync(1))
            .ReturnsAsync(Array.Empty<PerfumeListDto>());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.GetLists();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetListsOverview_returns_200()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.GetListsOverviewAsync(1))
            .ReturnsAsync(Array.Empty<PerfumeListOverviewDto>());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.GetListsOverview();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateList_returns_200_and_forwards_name()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.CreateListAsync(1, "My List"))
            .ReturnsAsync(new PerfumeListDto());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.CreateList(new CreateListRequest("My List"));

        result.Should().BeOfType<OkObjectResult>();

        lists.Verify(
            x => x.CreateListAsync(1, "My List"),
            Times.Once);
    }

    [Fact]
    public async Task RenameList_returns_204()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.RenameList(10, new RenameListRequest("New Name"));

        result.Should().BeOfType<NoContentResult>();

        lists.Verify(
            x => x.RenameListAsync(1, 10, "New Name"),
            Times.Once);
    }

    [Fact]
    public async Task DeleteList_returns_204()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.DeleteList(5);

        result.Should().BeOfType<NoContentResult>();

        lists.Verify(
            x => x.DeleteListAsync(1, 5),
            Times.Once);
    }

    [Fact]
    public async Task GetListPerfumes_returns_200()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.GetListPerfumesAsync(1, 2))
            .ReturnsAsync(Array.Empty<PerfumeListItemDto>());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.GetListPerfumes(2);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddPerfume_returns_204()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.AddPerfume(3, 7);

        result.Should().BeOfType<NoContentResult>();

        lists.Verify(
            x => x.AddPerfumeToListAsync(1, 3, 7),
            Times.Once);
    }

    [Fact]
    public async Task RemovePerfume_returns_204()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.RemovePerfume(3, 7);

        result.Should().BeOfType<NoContentResult>();

        lists.Verify(
            x => x.RemovePerfumeFromListAsync(1, 3, 7),
            Times.Once);
    }

    [Fact]
    public async Task GetListsForPerfume_returns_200()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.GetListsForPerfumeAsync(1, 9))
            .ReturnsAsync(
                new List<PerfumeListMembershipDto>());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.GetListsForPerfume(9);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetList_returns_200()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        lists
            .Setup(x => x.GetListAsync(1, 4))
            .ReturnsAsync(new PerfumeListDto());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.GetList(4);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Share_returns_200_and_calls_shared_service()
    {
        var lists = new Mock<IPerfumeListService>();
        var shared = new Mock<ISharedListService>();

        shared
            .Setup(x => x.ShareAsync(1, 6))
            .ReturnsAsync(new SharedListDto());

        var sut = CreateSut(lists, shared, User(1));

        var result = await sut.Share(6);

        result.Result.Should().BeOfType<OkObjectResult>();

        shared.Verify(
            x => x.ShareAsync(1, 6),
            Times.Once);
    }
}
