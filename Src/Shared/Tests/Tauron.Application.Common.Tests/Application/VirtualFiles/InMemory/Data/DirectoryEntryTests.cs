using System;
using FluentAssertions;
using NSubstitute;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.InMemory.Data;

public class DirectoryEntryTests
{
    [Fact]
    public void Simple_Add_Test()
    {
        var data = new DirectoryEntry();
        var toTest = new DirectoryEntry();

        var result = data.GetOrAdd("Test", () => toTest);
        
        result.Should().Be(toTest);
    }

    [Fact]
    public void Duplicate_Add_Test()
    {
        var data = new DirectoryEntry();
        var toTest = new DirectoryEntry();
        var duplicate = new DirectoryEntry();
        var testName = "Test";

        var result1 = data.GetOrAdd(testName, () => toTest);
        var result2 = data.GetOrAdd(testName, () => duplicate);

        result1.Should().Be(toTest);
        result2.Should().Be(toTest);
    }

    [Fact]
    public void Duplicate_TypeMismatch_Add_Test()
    {
        var data = new DirectoryEntry();
        var toTest = new DirectoryEntry();
        var duplicate = new FileEntry();
        var testName = "Test";

        var result1 = data.GetOrAdd(testName, () => toTest);
        Action result2 = () => data.GetOrAdd(testName, () => duplicate);

        result1.Should().Be(toTest);
        result2.Should().Throw<InvalidCastException>();
    }
        
    [Fact]
    public void File_Enumerator_Test()
    {
        var data = new DirectoryEntry();
        var testFile = new FileEntry();
            
        data.GetOrAdd("Test", () => testFile);

        data.Files.Should().HaveCount(1).And.Contain(testFile);
        data.Directorys.Should().HaveCount(0);
    }

    [Fact]
    public void Directory_Enumerator_Test()
    {
        var data = new DirectoryEntry();
        var testDic = new DirectoryEntry();

        data.GetOrAdd("Test", () => testDic);

        data.Files.Should().HaveCount(0);
        data.Directorys.Should().HaveCount(1).And.Contain(testDic);
    }

    [Fact]
    public void Remove_Test()
    {
        var data = new DirectoryEntry();
        var testAdd = new DirectoryEntry();
        var testName = "Test";
            
        data.GetOrAdd(testName, () => testAdd);
        var result = data.Remove(testName);

        result.Should().BeTrue();
        data.Directorys.Should().HaveCount(0);
    }

    [Fact]
    public void Dispose_Test()
    {
        var data = new DirectoryEntry();
        var mockData = Substitute.For<IDataElement>();

        data.GetOrAdd("Test", () => mockData);
        data.Dispose();
            
        mockData.Received(1).Dispose();
    }
}