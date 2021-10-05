using System;
using System.IO;
using FluentAssertions;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory.Data
{
    public class FileEntryTests
    {
        
        [Fact]
        public void Valid_Init_Test()
        {
            using var testStream = new MemoryStream();
            var testName = "Test";
            var testData = new FileEntry();
            
            testData.Init(testName, testStream);

            testData.Name.Should().Be(testName);
            testData.Data.Should().BeSameAs(testStream);
        }

        [Fact]
        public void InValid_Init_Test()
        {
            var testStream = new MemoryStream();
            var invalidName = string.Empty;
            var testData = new FileEntry();

            testData.Invoking(d => d.Init(invalidName, testStream))
               .Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Dispose_Test()
        {
            var stream = new MemoryStream();
            var name = "Test_Data";
            var data = new FileEntry();
            
            Action act = () => stream.WriteByte(1);
            data.Init(name, stream);
            data.Dispose();

            data.Data.Should().BeNull();
            data.Name.Should().BeNull();
            act.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void Non_Init_Actual_Test()
        {
            var data = new FileEntry();

            Func<string> act = () => data.ActualName;
            Func<MemoryStream> act2 = () => data.ActualData;

            act.Should().Throw<InvalidOperationException>();
            act2.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Init_Actual_Test()
        {
            using var stream = new MemoryStream();
            var name = "Test";
            var data = new FileEntry();
            
            data.Init(name, stream);
            var actualStream = data.ActualData;
            var actualName = data.ActualName;

            actualStream.Should().BeSameAs(stream);
            actualName.Should().Be(name);
        }
    }
}