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
        const string testRootPath = "mem::Test/Test2";
        const string testDic = "Test3/Test4";
        const string testFile = "Test5/Test.txt";
        string[] movedFiles = { "mem::Test6.text", "mem::Test6/Test7.text", "mem::Test6", "mem::Test6/Test7" };
        (string, string)[] movedDics =
        {
            ("mem::TestXX", "TestXX"), ("mem::TestXX/TestXXX", "TestXX/TestXXX"), ("TestXX", "TestXX"), ("TestXX/TestXXX", "TestXX/TestXXX")
        };
        const string testContent = "Hello World";
        
        using IVirtualFileSystem system = fac.TryResolve(testRootPath, new DeterministicServiceProvider((typeof(ISystemClock), CpuClock.Instance)))!;
        system.Should().NotBeNull();
        system.OriginalPath.Path.Should().Be(testRootPath);

        var dic = system.GetDirectory(testDic);
        dic.OriginalPath.Path.Should().StartWith(testRootPath).And.EndWith(testDic);

        var file = dic.GetFile(testFile);
        file.OriginalPath.Path.Should().StartWith(testRootPath).And.Contain(testDic).And.EndWith(testFile);
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

        foreach (var movedFile in movedFiles)
        {
            var simpleMoveFile = file.MoveTo(GenericPathHelper.ToRelativePath(movedFile));

            file.Exist.Should().BeFalse();
            // ReSharper disable once AccessToModifiedClosure
            Func<string> test = () => file.Extension;
            test.Should().Throw<InvalidOperationException>();

            var newFile = simpleMoveFile.MoveTo(movedFile);

            newFile.OriginalPath.Path.Should().StartWith(testRootPath).And.EndWith(GenericPathHelper.ToRelativePath(movedFile)).And.NotContain(testDic);
            newFile.Extension.Should().Be(".text");

            using (var file4 = new StreamReader(newFile.Open()))
                file4.ReadToEnd().Should().Be(testContent);

            newFile.Delete();

            newFile.Exist.Should().BeFalse();
            // ReSharper disable once AccessToModifiedClosure
            Func<string> newFileTest = () => file.Extension;
            newFileTest.Should().Throw<InvalidOperationException>();

            file = newFile;
        }

        foreach (var (newPath, excpected) in movedDics)
        {
            var newDic = dic.MoveTo(newPath);

            dic.Exist.Should().BeFalse();
            // ReSharper disable once AccessToModifiedClosure
            Func<string> oldPath = () => dic.Name;
            oldPath.Should().Throw<InvalidOperationException>();

            newDic.Exist.Should().BeTrue();
            excpected.Should().Contain(newDic.Name);
            newDic.OriginalPath.Path.Should().StartWith(testRootPath).And.EndWith(excpected);
            
            dic = newDic;
        }
    }
}