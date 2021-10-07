using System;
using System.Reactive.PlatformServices;
using NSubstitute;

namespace Test.Helper;

public static class MockHelper
{
    public static ISystemClock CreateStaticClock()
        => Substitute.For<ISystemClock>();
        
    public static ISystemClock CreateStaticClock(DateTimeOffset time)
    {
        var mock = Substitute.For<ISystemClock>();
        mock.UtcNow.Returns(time);

        return mock;
    }
        
    public static ISystemClock CreateStaticClock(DateTimeOffset time, params DateTimeOffset[] additional)
    {
        var mock = Substitute.For<ISystemClock>();
        mock.UtcNow.Returns(time, additional);

        return mock;
    }
}