using Api.Controllers;
using Core.DTOs;
using FluentAssertions;
using Infrastructure.Data;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class GroupsControllerTests
{
    private static GroupsController CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task Dictionary_returns_all_groups_ordered_by_name()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        ctx.AddRange(
            GroupBuilder.Default().WithId(1).WithName("Woody").Build(),
            GroupBuilder.Default().WithId(2).WithName("Amber").Build(),
            GroupBuilder.Default().WithId(3).WithName("Fresh").Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).Dictionary();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<List<GroupDictionaryItemDto>>().Subject;

        items.Select(x => x.Name)
            .Should()
            .Equal("Amber", "Fresh", "Woody");

        items.Select(x => x.Id)
            .Should()
            .Equal(2, 3, 1);
    }
}
