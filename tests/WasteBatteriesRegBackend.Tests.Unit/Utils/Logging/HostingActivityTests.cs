using System.Diagnostics;
using WasteBatteriesRegBackend.Utils.Logging;

namespace WasteBatteriesRegBackend.Tests.Unit.Utils.Logging;

public class HostingActivityTests
{
    [Fact]
    public void Listen_WhenParentIdIsNotW3C_RecordsTheActivity()
    {
        HostingActivity.Listen();

        using var source = new ActivitySource("Microsoft.AspNetCore");
        using var activity = source.StartActivity("request", ActivityKind.Server, parentId: "|not-a-w3c-id.");

        // A hierarchical parent has no W3C trace id. Recording is what shows the listener sampled it.
        Assert.NotNull(activity);
        Assert.True(activity.Recorded);
    }
}
