using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WasteBatteriesRegBackend.Utils.Logging;

namespace WasteBatteriesRegBackend.Tests.Unit.Utils.Logging;

public class ProblemDetailsTraceIdTests
{
    [Fact]
    public void Select_WhenRequestTraceIdIsPresent_UsesRequestTraceId()
    {
        var traceId = ProblemDetailsTraceId.Select(
            "trace-mismatch-check-001",
            "e9553a6557614e09143838699e68851a",
            "request-7");

        Assert.Equal("trace-mismatch-check-001", traceId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Select_WhenRequestTraceIdIsBlank_UsesActivityTraceId(string? requestTraceId)
    {
        var traceId = ProblemDetailsTraceId.Select(
            requestTraceId,
            "e9553a6557614e09143838699e68851a",
            "request-7");

        Assert.Equal("e9553a6557614e09143838699e68851a", traceId);
    }

    [Fact]
    public void Select_WhenRequestAndActivityTraceIdsAreBlank_UsesRequestIdentifier()
    {
        var traceId = ProblemDetailsTraceId.Select(null, null, "request-7");

        Assert.Equal("request-7", traceId);
    }

    [Fact]
    public void Select_WhenRequestIdentifierIsATraceParent_UsesItsTraceId()
    {
        var traceId = ProblemDetailsTraceId.Select(
            null,
            null,
            "00-E9553A6557614E09143838699E68851A-19fbdea7fac6df4c-01");

        Assert.Equal("e9553a6557614e09143838699e68851a", traceId);
    }

    [Fact]
    public void Select_WhenRequestIdentifierIsALowerCaseTraceParent_UsesItsTraceId()
    {
        var traceId = ProblemDetailsTraceId.Select(
            null,
            null,
            "00-e9553a6557614e09143838699e68851a-19fbdea7fac6df4c-01");

        Assert.Equal("e9553a6557614e09143838699e68851a", traceId);
    }

    [Theory]
    [InlineData("00-E9553A6557614E09143838699E68851Z-19fbdea7fac6df4c-01")]
    [InlineData("00-g9553a6557614e09143838699e68851a-19fbdea7fac6df4c-01")]
    [InlineData("00-/9553A6557614E09143838699E68851A-19fbdea7fac6df4c-01")]
    [InlineData("00-:9553A6557614E09143838699E68851A-19fbdea7fac6df4c-01")]
    [InlineData("00XE9553A6557614E09143838699E68851A-19fbdea7fac6df4c-01")]
    [InlineData("00-E9553A6557614E09143838699E68851AX19fbdea7fac6df4c-01")]
    [InlineData("00-E9553A6557614E09143838699E68851A-19fbdea7fac6df4cX01")]
    public void Select_WhenRequestIdentifierIsNotATraceParent_UsesRequestIdentifier(string requestIdentifier)
    {
        var traceId = ProblemDetailsTraceId.Select(null, null, requestIdentifier);

        Assert.Equal(requestIdentifier, traceId);
    }

    [Fact]
    public void Apply_WhenNoActivityIsCurrent_ReadsTheTraceIdFromTheRequestIdentifier()
    {
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            var context = Context();
            context.HttpContext.TraceIdentifier = "00-E9553A6557614E09143838699E68851A-19fbdea7fac6df4c-01";

            ProblemDetailsTraceId.Apply(context, null);

            Assert.Equal("e9553a6557614e09143838699e68851a", context.ProblemDetails.Extensions["traceId"]);
        }
        finally
        {
            Activity.Current = previous;
        }
    }

    [Fact]
    public void Apply_WhenActivityTraceIdIsUnset_UsesRequestIdentifier()
    {
        using var activity = new Activity("unset").Start();
        // Start() will not keep an all-zero trace id, which is the value this branch ignores.
        typeof(Activity)
            .GetField("_traceId", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(activity, "00000000000000000000000000000000");
        var context = Context();

        ProblemDetailsTraceId.Apply(context, null);

        Assert.Equal("request-7", context.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void Apply_WhenTraceHeaderIsPresent_UsesHeader()
    {
        using var activity = new Activity("test").Start();
        var context = Context();
        context.HttpContext.Request.Headers["x-cdp-request-id"] = "trace-mismatch-check-001";

        ProblemDetailsTraceId.Apply(context, "x-cdp-request-id");

        Assert.Equal("trace-mismatch-check-001", context.ProblemDetails.Extensions["traceId"]);
        Assert.NotEqual(activity.TraceId.ToHexString(), context.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void Apply_WhenTraceHeaderIsAbsent_UsesActivityTraceId()
    {
        using var activity = new Activity("test").Start();
        var context = Context();

        ProblemDetailsTraceId.Apply(context, "x-cdp-request-id");

        Assert.Equal(activity.TraceId.ToHexString(), context.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void Apply_WhenTraceHeaderIsBlank_UsesActivityTraceId()
    {
        using var activity = new Activity("test").Start();
        var context = Context();
        context.HttpContext.Request.Headers["x-cdp-request-id"] = "   ";

        ProblemDetailsTraceId.Apply(context, "x-cdp-request-id");

        Assert.Equal(activity.TraceId.ToHexString(), context.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void Apply_WhenHeaderNameIsNotConfigured_IgnoresHeader()
    {
        using var activity = new Activity("test").Start();
        var context = Context();
        context.HttpContext.Request.Headers["x-cdp-request-id"] = "trace-mismatch-check-001";

        ProblemDetailsTraceId.Apply(context, null);

        Assert.Equal(activity.TraceId.ToHexString(), context.ProblemDetails.Extensions["traceId"]);
    }

    private static ProblemDetailsContext Context()
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "request-7"
        };
        return new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails()
        };
    }
}
