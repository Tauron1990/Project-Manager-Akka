using FluentAssertions;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Test.Helper;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory;

public class InMemoryDirectoryTests
{
    private static DirectoryContext CreateSimpleContext()
    {
        var clock = MockHelper.CreateStaticClock();
        var root = new InMemoryRoot();

        return new DirectoryContext(root, null, root.GetDirectoryEntry("Test", clock), "Test", clock, new InMemoryFileSystem(clock, "mem"));
    }

    [Fact]
    public void GetDirecory_Test()
    {
        var context = CreateSimpleContext();
        var dic = new InMemoryDirectory(context, FileSystemFeature.None);

        var dic2 = dic.GetDirectory($"Test2{GenericPathHelper.GenericSeperator}Test3");

        dic2.OriginalPath.Should().Be($"Test{GenericPathHelper.GenericSeperator}Test2{GenericPathHelper.GenericSeperator}Test3");
    }
    
    [Fact]
    public void GetFile_Test()
    {
        var context = CreateSimpleContext();
        var dic = new InMemoryDirectory(context, FileSystemFeature.None);

        var dic2 = dic.GetFile($"Test2{GenericPathHelper.GenericSeperator}Test3.dat");

        dic2.OriginalPath.Should().Be($"Test{GenericPathHelper.GenericSeperator}Test2{GenericPathHelper.GenericSeperator}Test3.dat");
    }
}