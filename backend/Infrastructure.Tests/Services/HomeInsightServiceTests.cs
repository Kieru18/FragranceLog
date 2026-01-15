using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services
{
    public sealed class HomeInsightServiceTests
    {
        private static HomeInsightDto Global(string key) =>
            new()
            {
                Key = key,
                Title = $"Global {key}",
                Subtitle = "Subtitle",
                Icon = InsightIconEnum.Compass,
                Scope = InsightScopeEnum.Global
            };

        private static HomeInsightDto Personal(string key) =>
            new()
            {
                Key = key,
                Title = $"Personal {key}",
                Subtitle = "Subtitle",
                Icon = InsightIconEnum.Heart,
                Scope = InsightScopeEnum.Personal
            };

        [Fact]
        public async Task GetInsightsAsync_ShouldReturnEmpty_WhenNoProviders()
        {
            var service = new HomeInsightService(Array.Empty<IHomeInsightProvider>());

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldIgnoreNullInsights()
        {
            var provider = new Mock<IHomeInsightProvider>();
            provider.Setup(p => p.TryBuildAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((HomeInsightDto?)null);

            var service = new HomeInsightService(new[] { provider.Object });

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldInvokeAllProviders()
        {
            var p1 = new Mock<IHomeInsightProvider>();
            var p2 = new Mock<IHomeInsightProvider>();

            p1.Setup(p => p.TryBuildAsync(1, It.IsAny<CancellationToken>()))
              .ReturnsAsync(Global("g1"));

            p2.Setup(p => p.TryBuildAsync(1, It.IsAny<CancellationToken>()))
              .ReturnsAsync(Personal("p1"));

            var service = new HomeInsightService(new[]
            {
                p1.Object,
                p2.Object
            });

            await service.GetInsightsAsync(1, CancellationToken.None);

            p1.Verify(p => p.TryBuildAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            p2.Verify(p => p.TryBuildAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldLimitGlobalInsightsToTwo()
        {
            var providers = Enumerable.Range(1, 5)
                .Select(i => Provider(Global($"g{i}")))
                .ToList();

            var service = new HomeInsightService(providers);

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Count(i => i.Scope == InsightScopeEnum.Global)
                  .Should().BeLessThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldLimitPersonalInsightsToTwo()
        {
            var providers = Enumerable.Range(1, 5)
                .Select(i => Provider(Personal($"p{i}")))
                .ToList();

            var service = new HomeInsightService(providers);

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Count(i => i.Scope == InsightScopeEnum.Personal)
                  .Should().BeLessThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldReturnAtMostFourInsights()
        {
            var providers = new List<IHomeInsightProvider>();

            for (var i = 0; i < 10; i++)
            {
                providers.Add(
                    Provider(i % 2 == 0
                        ? Global($"g{i}")
                        : Personal($"p{i}")));
            }

            var service = new HomeInsightService(providers);

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Should().HaveCount(c => c <= 4);
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldOnlyReturnInsightsProducedByProviders()
        {
            var produced = new[]
            {
                Global("g1"),
                Global("g2"),
                Personal("p1"),
                Personal("p2")
            };

            var providers = produced
                .Select(Provider)
                .ToList();

            var service = new HomeInsightService(providers);

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Should().OnlyContain(i => produced.Contains(i));
        }

        [Fact]
        public async Task GetInsightsAsync_ShouldRespectScopeSeparation()
        {
            var providers = new IHomeInsightProvider[]
            {
                Provider(Global("g1")),
                Provider(Global("g2")),
                Provider(Global("g3")),
                Provider(Personal("p1")),
                Provider(Personal("p2")),
                Provider(Personal("p3"))
            };

            var service = new HomeInsightService(providers);

            var result = await service.GetInsightsAsync(1, CancellationToken.None);

            result.Count(i => i.Scope == InsightScopeEnum.Global)
                  .Should().BeLessThanOrEqualTo(2);

            result.Count(i => i.Scope == InsightScopeEnum.Personal)
                  .Should().BeLessThanOrEqualTo(2);
        }

        private static IHomeInsightProvider Provider(HomeInsightDto insight)
        {
            var mock = new Mock<IHomeInsightProvider>();
            mock.Setup(p => p.TryBuildAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(insight);
            return mock.Object;
        }
    }
}
