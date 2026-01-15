using Core.Interfaces;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Infrastructure.Tests.Services
{
    public sealed class GeoServiceTests
    {
        private static HttpClient CreateClient(
            HttpStatusCode status,
            string content,
            Action<HttpRequestMessage>? assertRequest = null)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
                {
                    assertRequest?.Invoke(req);

                    return new HttpResponseMessage
                    {
                        StatusCode = status,
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    };
                });

            return new HttpClient(handler.Object);
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldReturnCountryCode_WhenResponseIsValid()
        {
            var json =
                """
                {
                  "address": {
                    "country_code": "pl"
                  }
                }
                """;

            var client = CreateClient(HttpStatusCode.OK, json);
            var service = new GeoService(client);

            var result = await service.ResolveCountryAsync(52.1, 21.0, CancellationToken.None);

            result.Should().Be("PL");
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldUseInvariantCulture_ForCoordinates()
        {
            var json =
                """
                {
                  "address": {
                    "country_code": "de"
                  }
                }
                """;

            HttpRequestMessage? captured = null;

            var client = CreateClient(
                HttpStatusCode.OK,
                json,
                req => captured = req);

            var service = new GeoService(client);

            await service.ResolveCountryAsync(52.1234, 21.5678, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.RequestUri!.Query.Should().Contain("lat=52.1234");
            captured!.RequestUri!.Query.Should().Contain("lon=21.5678");
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldReturnNull_WhenStatusIsNotSuccess()
        {
            var client = CreateClient(HttpStatusCode.BadRequest, "{}");
            var service = new GeoService(client);

            var result = await service.ResolveCountryAsync(0, 0, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldReturnNull_WhenAddressIsMissing()
        {
            var json = "{}";

            var client = CreateClient(HttpStatusCode.OK, json);
            var service = new GeoService(client);

            var result = await service.ResolveCountryAsync(0, 0, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldReturnNull_WhenCountryCodeIsMissing()
        {
            var json =
                """
                {
                  "address": {}
                }
                """;

            var client = CreateClient(HttpStatusCode.OK, json);
            var service = new GeoService(client);

            var result = await service.ResolveCountryAsync(0, 0, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ResolveCountryAsync_ShouldReturnNull_WhenCountryCodeIsNull()
        {
            var json =
                """
                {
                  "address": {
                    "country_code": null
                  }
                }
                """;

            var client = CreateClient(HttpStatusCode.OK, json);
            var service = new GeoService(client);

            var result = await service.ResolveCountryAsync(0, 0, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
