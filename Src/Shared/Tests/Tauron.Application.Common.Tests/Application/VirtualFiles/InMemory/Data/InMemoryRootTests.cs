using System;
using FluentAssertions;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Test.Helper;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory.Data;

public class InMemoryRootTests
{
    private readonly DateTime _testDate = new(2015, 5, 5);
        
    [Fact]
    public void Simple_GetFile_Test()
    {
        using var data = new InMemoryRoot();
        var clock = MockHelper.CreateStaticClock(_testDate);
            
        var file = data.GetInitializedFile("Test", clock);
        data.ReturnFile(file);
        var file2 = data.GetInitializedFile("Test2", clock);
            
        file2.ActualName.Should().Be("Test2");
    }

    [Fact]
    public void Simple_GetDirectory_Test()
    {
        using var data = new InMemoryRoot();
        var clock = MockHelper.CreateStaticClock(_testDate);
            
        var dic = data.GetDirectoryEntry("Test", clock);
        data.ReturnDirectory(dic);
        var dic2 = data.GetDirectoryEntry("Test2", clock);

        dic2.Name.Should().Be("Test2");
    }

    [Fact]
    public void Invalid_Name_GetDirectory_Test()
    {
        using var data = new InMemoryRoot();
        var clock = MockHelper.CreateStaticClock(_testDate);
            
        // ReSharper disable AccessToDisposedClosure
        Func<DirectoryEntry> act = () => data.GetDirectoryEntry(string.Empty, clock);
        Func<DirectoryEntry> act2 = () => data.GetDirectoryEntry(null!, clock);

        act.Should().ThrowExactly<InvalidOperationException>();
        act2.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Invalid_Directory_Return_Test()
    {
        using var data = new InMemoryRoot();

        // ReSharper disable AccessToDisposedClosure
        Action act = () => data.ReturnDirectory(data);

        act.Should().ThrowExactly<InvalidOperationException>();
    }
}