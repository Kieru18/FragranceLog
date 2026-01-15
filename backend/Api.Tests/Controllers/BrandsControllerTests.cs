using Api.Controllers;
using Core.DTOs;
using FluentAssertions;
using Infrastructure.Data;
using Tests.Common;
using Tests.Common.Builders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class BrandsControllerTests
{
    private static BrandsController CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task Dictionary_returns_all_brands_ordered_by_name()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var b1 = BrandBuilder.Default().WithId(1).WithName("Zara").Build();
        var b2 = BrandBuilder.Default().WithId(2).WithName("Armani").Build();
        var b3 = BrandBuilder.Default().WithId(3).WithName("Dior").Build();

        ctx.AddRange(b1, b2, b3);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).Dictionary();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var items = ok.Value.Should().BeAssignableTo<List<BrandDictionaryItemDto>>().Which;

        items.Select(x => x.Name)
            .Should()
            .Equal("Armani", "Dior", "Zara");
    }

    [Fact]
    public async Task Dictionary_returns_empty_list_when_no_brands_exist()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var result = await CreateSut(ctx).Dictionary();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var items = ok.Value.Should().BeAssignableTo<List<BrandDictionaryItemDto>>().Which;

        items.Should().BeEmpty();
    }

    [Fact]
    public void Controller_is_authorized()
    {
        typeof(BrandsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should()
            .NotBeEmpty();
    }
}
