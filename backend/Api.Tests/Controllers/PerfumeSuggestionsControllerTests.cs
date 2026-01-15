using Api.Controllers;
using Core.DTOs;
using Core.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class PerfumeSuggestionsControllerTests
{
    private static PerfumeSuggestionsController CreateSut(
        HttpResponseMessage response,
        IConfiguration? config = null,
        ClaimsPrincipal? user = null)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handler.Object);

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>()))
               .Returns(client);

        config ??= new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:WebhookUrl"] = "https://discord.test/webhook"
            })
            .Build();

        var controller = new PerfumeSuggestionsController(factory.Object, config);

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

    private static ClaimsPrincipal User(string id = "123")
        => new(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, id) },
            "test"));

    private static PerfumeSuggestionRequestDto CreateDto(
        string brand = "Dior",
        string name = "Sauvage",
        string? imageBase64 = null)
        => new()
        {
            Brand = brand,
            Name = name,
            ImageBase64 = imageBase64,
            Groups = ["Fresh"],
            NoteGroups =
            [
                new()
                {
                    Type = NoteTypeEnum.Top,
                    Notes = ["Bergamot"]
                }
            ]
        };

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        var attr = typeof(PerfumeSuggestionsController)
            .GetCustomAttribute<AuthorizeAttribute>();

        attr.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_bad_request_when_brand_or_name_missing()
    {
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.OK),
            user: User());

        var dto = CreateDto(brand: "");

        var result = await sut.Create(dto);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Create_500_when_webhook_not_configured()
    {
        var config = new ConfigurationBuilder().Build();

        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.OK),
            config,
            User());

        var result = await sut.Create(CreateDto());

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_bad_request_when_image_base64_too_large()
    {
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.OK),
            user: User());

        var dto = CreateDto(
            imageBase64: new string('A', 12_000_001));

        var result = await sut.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_bad_request_when_unsupported_image_format()
    {
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.OK),
            user: User());

        var fakeBytes = new byte[] { 0x00, 0x00, 0x00 };

        var dto = CreateDto(
            imageBase64: Convert.ToBase64String(fakeBytes));

        var result = await sut.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_returns_502_when_discord_fails()
    {
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.BadGateway),
            user: User());

        var result = await sut.Create(CreateDto());

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(502);
    }

    [Fact]
    public async Task Create_returns_accepted_on_success()
    {
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.NoContent),
            user: User("42"));

        var result = await sut.Create(CreateDto());

        result.Should().BeOfType<AcceptedResult>();
    }
}
