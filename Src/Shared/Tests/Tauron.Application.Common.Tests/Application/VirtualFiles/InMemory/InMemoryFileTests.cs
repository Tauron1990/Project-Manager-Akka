using System;
using System.IO;
using System.Reactive.PlatformServices;
using FluentAssertions;
using NSubstitute;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.InMemory;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Test.Helper;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory;

public class InMemoryFileTests
{
    private static FileContext CreateSimpleContext(string name, ISystemClock clock)
    {
        var dataRoot = new InMemoryRoot();

        return new FileContext(dataRoot, null, dataRoot.GetInitializedFile(name, clock), name, clock, Substitute.For<IVirtualFileSystem>());
    }

    [Fact]
    public void Get_Extension_Test()
    {
        const string fileName = "file.dat";
        var clock = MockHelper.CreateStaticClock();
        var context = CreateSimpleContext(fileName, clock);
        var met = new InMemoryFile(context, FileSystemFeature.None);

        var ext = met.Extension;

        ext.Should().Be(".dat");
    }
        
    [Fact]
    public void ChangeExtension_Test()
    {
        const string fileName = "Test.dat";
        const string newFileExtension = ".dot";
        var modifyDate = new DateTime(2020, 11, 11);
        var clock = MockHelper.CreateStaticClock(new DateTime(2020, 10, 10), modifyDate);
        var context = CreateSimpleContext(fileName, clock);
        var data = new InMemoryFile(context, FileSystemFeature.Extension);

        data.Extension = newFileExtension;

        data.Extension.Should().Be(".dot");
        data.LastModified.Should().Be(modifyDate);
    }

    [Fact]
    public void CreateNew_Stream_Test()
    {
        var context = CreateSimpleContext("Test", MockHelper.CreateStaticClock());
        var file = new InMemoryFile(context, FileSystemFeature.Write | FileSystemFeature.Read | FileSystemFeature.Create);

        using var stream = file.CreateNew();
        stream.WriteByte(1);
        using var stream2 = file.CreateNew();

        // ReSharper disable once AccessToDisposedClosure
        new Func<long>(() => stream.Length).Should().ThrowExactly<ObjectDisposedException>();
        stream2.Length.Should().Be(0);
    }

    [Fact]
    public void Create_Stream_Test()
    {
        var context = CreateSimpleContext("Test", MockHelper.CreateStaticClock());
        var file = new InMemoryFile(context, FileSystemFeature.Read | FileSystemFeature.Write);

        using var stream1 = file.Open();
        stream1.WriteByte(1);
        using var stream2 = file.Open(FileAccess.Read);

        stream1.Length.Should().Be(1);
        stream1.CanRead.Should().BeTrue();
        stream1.CanWrite.Should().BeTrue();
            
        stream2.Length.Should().Be(1);
        stream2.CanRead.Should().BeTrue();
        stream2.CanWrite.Should().BeFalse();
            
    }
}