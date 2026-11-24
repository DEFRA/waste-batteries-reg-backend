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

        Assert.NotNull(activity);
        Assert.True(activity.Recorded);
        Assert.NotEqual("00000000000000000000000000000000", activity.TraceId.ToHexString());
    }
}
