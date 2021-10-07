using System;
using FluentAssertions;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory.Data
{
    public class InMemoryRootTests
    {
        [Fact]
        public void Simple_GetFile_Test()
        {
            using var data = new InMemoryRoot();

            var file = data.GetInitializedFile("Test");
            data.ReturnFile(file);
            var file2 = data.GetInitializedFile("Test2");

            file2.Should().BeSameAs(file);
            file2.ActualName.Should().Be("Test2");
        }

        [Fact]
        public void Simple_GetDirectory_Test()
        {
            using var data = new InMemoryRoot();

            var dic = data.GetDirectoryEntry("Test");
            data.ReturnDirectory(dic);
            var dic2 = data.GetDirectoryEntry("Test2");

            dic2.Should().Be(dic);
            dic2.Name.Should().Be("Test2");
        }

        [Fact]
        public void Invalid_Name_GetDirectory_Test()
        {
            using var data = new InMemoryRoot();

            // ReSharper disable AccessToDisposedClosure
            Func<DirectoryEntry> act = () => data.GetDirectoryEntry(string.Empty);
            Func<DirectoryEntry> act2 = () => data.GetDirectoryEntry(null!);

            act.Should().ThrowExactly<InvalidOperationException>();
            act2.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Invalid_Directory_Return_Test()
        {
            using var data = new InMemoryRoot();

            // ReSharper disable AccessToDisposedClosure
            Action act = () => data.ReturnDirectory(data);

            act.Should().ThrowExactly<InvalidCastException>();
        }
    }
}