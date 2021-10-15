using System;
using System.IO;
using System.Reactive.PlatformServices;
using FluentAssertions;
using Stl.Time;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;
using Test.Helper;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory;

public class VirtualFileSystemTest
{
    [Fact]
    public void TestInMemoryVirtualFileSystem()
    {
        var fac = new VirtualFileFactory();
        const string testPath = "mem::Test/Test2";
        const string testDic = "Test3/Test4";
        const string testFile = "Test5/Test.txt";
        const string movedFile = "mem::Test6.text";
        const string testContent = "Hello World";
        
        using IVirtualFileSystem system = fac.TryResolve(testPath, new DeterministicServiceProvider((typeof(ISystemClock), CpuClock.Instance)))!;
        system.Should().NotBeNull();
        system.OriginalPath.Path.Should().Be(testPath);

        var dic = system.GetDirectory(testDic);
        dic.OriginalPath.Path.Should().StartWith(testPath).And.EndWith(testDic);

        var file = dic.GetFile(testFile);
        file.OriginalPath.Path.Should().StartWith(testPath).And.Contain(testDic).And.EndWith(testFile);
        file.Extension.Should().Be(".txt");
        file.Exist.Should().BeTrue();
        
        using (var file1 = new StreamWriter(file.Open()))
            file1.Write(testContent);

        using (var file2 = file.Open(FileAccess.Read))
        {
            file2.Should().BeReadable().And.NotBeWritable().And.HavePosition(0);
            
            using var reader = new StreamReader(file2);
            reader.ReadToEnd().Should().Be(testContent);
        }

        using (var file3 = file.CreateNew())
        {
            file3.Should().BeReadable().And.BeWritable().And.HaveLength(0);

            using var writer = new StreamWriter(file3);
            writer.Write(testContent);
        }

        var simpleMoveFile = file.MoveTo(GenericPathHelper.ToRelativePath(movedFile));
        
        file.Exist.Should().BeFalse();
        Func<string> test = () => file.Extension;
        test.Should().Throw<InvalidOperationException>();
        
        var newFile = simpleMoveFile.MoveTo(movedFile);

        newFile.OriginalPath.Path.Should().StartWith(testPath).And.EndWith(movedFile).And.NotContain(testDic);
        newFile.Extension.Should().Be(".text");

        using (var file4 = new StreamReader(newFile.Open())) 
            file4.ReadToEnd().Should().Be(testContent);
        
        newFile.Delete();
        
        newFile.Exist.Should().BeFalse();
        Func<string> newFileTest = () => file.Extension;
        newFileTest.Should().Throw<InvalidOperationException>();
    }
}