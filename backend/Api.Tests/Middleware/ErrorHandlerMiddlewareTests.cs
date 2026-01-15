using System.Net;
using System.Text.Json;
using Api.Middleware;
using Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Tests.Middleware
{
    public sealed class ErrorHandlerMiddlewareTests
    {
        [Theory]
        [InlineData(typeof(ValidationException), HttpStatusCode.BadRequest)]
        [InlineData(typeof(UnauthorizedException), HttpStatusCode.Unauthorized)]
        [InlineData(typeof(ForbiddenException), HttpStatusCode.Forbidden)]
        [InlineData(typeof(NotFoundException), HttpStatusCode.NotFound)]
        [InlineData(typeof(ConflictException), HttpStatusCode.Conflict)]
        public async Task Invoke_ShouldMapKnownExceptions_ToExpectedStatusCode(
            Type exceptionType,
            HttpStatusCode expectedStatus)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, "error")!;

            RequestDelegate next = _ => throw exception;

            var logger = new Mock<ILogger<ErrorHandlerMiddleware>>();
            var middleware = new ErrorHandlerMiddleware(next, logger.Object);

            var context = CreateHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be((int)expectedStatus);
            context.Response.ContentType.Should().Be("application/json");

            var body = await ReadResponseBody(context);
            body.Should().Contain("\"error\":\"error\"");
            body.Should().Contain($"\"status\":{(int)expectedStatus}");
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerError_ForUnknownException()
        {
            RequestDelegate next = _ => throw new Exception("boom");

            var logger = new Mock<ILogger<ErrorHandlerMiddleware>>();
            var middleware = new ErrorHandlerMiddleware(next, logger.Object);

            var context = CreateHttpContext();

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var body = await ReadResponseBody(context);
            body.Should().Contain("\"error\":\"boom\"");
            body.Should().Contain($"\"status\":{(int)HttpStatusCode.InternalServerError}");
        }

        [Fact]
        public async Task Invoke_ShouldCallNext_WhenNoExceptionIsThrown()
        {
            var called = false;

            RequestDelegate next = _ =>
            {
                called = true;
                return Task.CompletedTask;
            };

            var logger = new Mock<ILogger<ErrorHandlerMiddleware>>();
            var middleware = new ErrorHandlerMiddleware(next, logger.Object);

            var context = CreateHttpContext();

            await middleware.Invoke(context);

            called.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        private static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<string> ReadResponseBody(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}
