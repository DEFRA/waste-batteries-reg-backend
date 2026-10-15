using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WasteBatteriesRegBackend.Example.Repositories;
using WasteBatteriesRegBackend.Test.Utils;
using WasteBatteriesRegBackend.Utils.Logging;

namespace WasteBatteriesRegBackend.Test.Utils.Logging;

public class ExceptionLoggingHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_logs_unhandled_exception_and_returns_problem_details()
    {
        var logger = new CapturingLogger<ExceptionLoggingHandler>();
        var problemDetailsService = Substitute.For<IProblemDetailsService>();
        ProblemDetailsContext? captured = null;
        problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => captured = context))
            .Returns(new ValueTask<bool>(true));

        var handler = new ExceptionLoggingHandler(logger, problemDetailsService);
        var exception = new InvalidOperationException("secret boom");
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(StatusCodes.Status500InternalServerError, captured.ProblemDetails.Status);
        Assert.Equal("An error occurred while processing your request.", captured.ProblemDetails.Title);
        Assert.Null(captured.ProblemDetails.Detail);
        var (level, logged) = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, level);
        Assert.Same(exception, logged);
    }

    [Fact]
    public async Task TryHandleAsync_returns_bad_request_problem_details()
    {
        var logger = new CapturingLogger<ExceptionLoggingHandler>();
        var problemDetailsService = Substitute.For<IProblemDetailsService>();
        ProblemDetailsContext? captured = null;
        problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => captured = context))
            .Returns(new ValueTask<bool>(true));

        var handler = new ExceptionLoggingHandler(logger, problemDetailsService);
        var httpContext = new DefaultHttpContext();

        await handler.TryHandleAsync(httpContext, new BadHttpRequestException("Malformed JSON."), CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(StatusCodes.Status400BadRequest, captured.ProblemDetails.Status);
        Assert.Equal("Bad request", captured.ProblemDetails.Title);
        Assert.Equal("Malformed JSON.", captured.ProblemDetails.Detail);
    }

    [Fact]
    public async Task Unhandled_endpoint_exception_returns_problem_details_without_the_exception_message()
    {
        const string exceptionMessage = "secret boom";
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateAuthenticatedClient();

        factory.MockPersistence
            .GetAllAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        var response = await client.GetAsync("/db-demo", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(body);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("An error occurred while processing your request.", problem.Title);
        Assert.DoesNotContain(exceptionMessage, body);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        public readonly IExampleRepository MockPersistence = Substitute.For<IExampleRepository>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestAuthentication();

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IExampleRepository>();
                services.AddSingleton(MockPersistence);
            });
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, exception));
    }
}
