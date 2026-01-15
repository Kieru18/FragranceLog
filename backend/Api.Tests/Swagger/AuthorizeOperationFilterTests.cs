using System.Linq;
using System.Reflection;
using Api.Swagger;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Api.Tests.Swagger
{
    public sealed class AuthorizeOperationFilterTests
    {
        private readonly AuthorizeOperationFilter _filter;

        public AuthorizeOperationFilterTests()
        {
            _filter = new AuthorizeOperationFilter();
        }

        [Fact]
        public void Apply_ShouldNotAddSecurity_WhenNoAuthorizeAttributeExists()
        {
            var operation = new OpenApiOperation();
            var context = CreateContext(typeof(NoAuthController).GetMethod(nameof(NoAuthController.Action))!);

            _filter.Apply(operation, context);

            operation.Security.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Apply_ShouldAddSecurity_WhenAuthorizeOnController()
        {
            var operation = new OpenApiOperation();
            var context = CreateContext(typeof(AuthorizedController).GetMethod(nameof(AuthorizedController.Action))!);

            _filter.Apply(operation, context);

            operation.Security.Should().NotBeNull();
            operation.Security.Should().HaveCount(1);
            operation.Security![0].Keys.Single().Reference!.Id.Should().Be("Bearer");
        }

        [Fact]
        public void Apply_ShouldAddSecurity_WhenAuthorizeOnMethod()
        {
            var operation = new OpenApiOperation();
            var context = CreateContext(typeof(MethodAuthorizedController).GetMethod(nameof(MethodAuthorizedController.Action))!);

            _filter.Apply(operation, context);

            operation.Security.Should().NotBeNull();
            operation.Security.Should().HaveCount(1);
            operation.Security![0].Keys.Single().Reference!.Id.Should().Be("Bearer");
        }

        [Fact]
        public void Apply_ShouldNotAddSecurity_WhenAllowAnonymousOverridesAuthorize()
        {
            var operation = new OpenApiOperation();
            var context = CreateContext(typeof(AllowAnonymousController).GetMethod(nameof(AllowAnonymousController.Action))!);

            _filter.Apply(operation, context);

            operation.Security.Should().BeNullOrEmpty();
        }

        private static OperationFilterContext CreateContext(MethodInfo method)
        {
            var apiDescription = new ApiDescription();
            var schemaGenerator = new Mock<ISchemaGenerator>().Object;
            var schemaRepository = new SchemaRepository();

            return new OperationFilterContext(
                apiDescription,
                schemaGenerator,
                schemaRepository,
                method
            );
        }

        private sealed class NoAuthController
        {
            public void Action() { }
        }

        [Authorize]
        private sealed class AuthorizedController
        {
            public void Action() { }
        }

        private sealed class MethodAuthorizedController
        {
            [Authorize]
            public void Action() { }
        }

        [Authorize]
        private sealed class AllowAnonymousController
        {
            [AllowAnonymous]
            public void Action() { }
        }
    }
}
