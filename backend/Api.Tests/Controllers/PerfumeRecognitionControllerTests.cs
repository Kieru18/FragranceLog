using Core.DTOs;
using Core.Interfaces;
using Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public sealed class PerfumeRecognitionControllerTests
{
    private static PerfumeRecognitionController CreateSut(
        Mock<IPerfumeRecognitionService> service)
        => new(service.Object);

    [Fact]
    public async Task Recognize_returns_bad_request_when_base64_is_empty()
    {
        var service = new Mock<IPerfumeRecognitionService>();
        var sut = CreateSut(service);

        var result = await sut.Recognize(
            new PerfumeRecognitionRequestDto
            {
                ImageBase64 = "",
                TopK = 3
            },
            default);

        result.Should().BeOfType<BadRequestResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Recognize_calls_service_and_returns_ok()
    {
        var service = new Mock<IPerfumeRecognitionService>();
        var expected = Array.Empty<PerfumeRecognitionResultDto>();

        service
            .Setup(x => x.RecognizeAsync(
                It.IsAny<Stream>(),
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut(service);

        var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        var result = await sut.Recognize(
            new PerfumeRecognitionRequestDto
            {
                ImageBase64 = base64,
                TopK = 3
            },
            default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(expected);

        service.Verify(
            x => x.RecognizeAsync(
                It.IsAny<Stream>(),
                3,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Recognize_swagger_returns_bad_request_when_file_is_null()
    {
        var service = new Mock<IPerfumeRecognitionService>();
        var sut = CreateSut(service);

        var result = await sut.Recognize(
            image: null!,
            topK: 3,
            ct: default);

        result.Should().BeOfType<BadRequestResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Recognize_swagger_returns_bad_request_when_file_is_empty()
    {
        var service = new Mock<IPerfumeRecognitionService>();
        var sut = CreateSut(service);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.Length).Returns(0);

        var result = await sut.Recognize(
            file.Object,
            topK: 3,
            ct: default);

        result.Should().BeOfType<BadRequestResult>();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Recognize_swagger_calls_service_and_returns_ok()
    {
        var service = new Mock<IPerfumeRecognitionService>();
        var expected = Array.Empty<PerfumeRecognitionResultDto>();

        service
            .Setup(x => x.RecognizeAsync(
                It.IsAny<Stream>(),
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut(service);

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var file = new Mock<IFormFile>();

        file.Setup(x => x.Length).Returns(3);
        file.Setup(x => x.OpenReadStream()).Returns(stream);

        var result = await sut.Recognize(
            file.Object,
            topK: 5,
            ct: default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(expected);

        service.Verify(
            x => x.RecognizeAsync(
                It.IsAny<Stream>(),
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
