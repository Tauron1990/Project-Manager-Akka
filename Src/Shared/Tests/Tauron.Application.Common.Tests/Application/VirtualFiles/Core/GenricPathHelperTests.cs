﻿using System.Collections.Generic;
using FluentAssertions;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;
using Xunit;

namespace Tauron.Application.Common.Tests.Application.VirtualFiles.Core;

public class GenricPathHelperTests
{
    public static IEnumerable<object[]> GetTestFileNamesForChangeExtensionTest()
    {
        yield return new object[] { GenericPathHelper.GenericSeperator, string.Empty, GenericPathHelper.GenericSeperator, "dat" };

        yield return new object[] { "test", string.Empty, "test.dat", "dat2" };
        yield return new object[] { "test2", "C:", $"C:{GenericPathHelper.GenericSeperator}test2.dat", "dat2" };
        yield return new object[] { "test3", "testpath", $"testpath{GenericPathHelper.GenericSeperator}test3.dat", "dat2" };
            
        yield return new object[] { "test4.tt", string.Empty, "test4.tt.dat", "dat2" };
        yield return new object[] { "test5.tt", "C:", $"C:{GenericPathHelper.GenericSeperator}test5.tt.dat", "dat2" };
        yield return new object[] { "test6.tt", "testpath", $"testpath{GenericPathHelper.GenericSeperator}test6.tt.dat", "dat2" };
    }

    [Theory]
    [MemberData(nameof(GetTestFileNamesForChangeExtensionTest))]
    public void Test_ChangeExtension(string targetName, string start, string pathToChange, string targetExt)
    {
        var newPath = GenericPathHelper.ChangeExtension(pathToChange, targetExt);

        newPath.Path.Should().Contain(targetName).And.StartWith(start).And.EndWith(targetExt);
    }
        

    [Fact]
    public void ChangeExtension_With_dot_in_provided_Extension()
    {
        const string targetPath = "test.dat";
        const string targetExtension = ".dot";

        var result = GenericPathHelper.ChangeExtension(targetPath, targetExtension);

        result.Path.Should().Be("test.dot");
    }

    [Fact]
    public void ChangeExtention_return_Empty_if_input_Empty()
    {
        var result = GenericPathHelper.ChangeExtension(string.Empty, "dat");

        result.Path.Should().BeEmpty();
    }
        
    public static IEnumerable<object[]> GetTestNamesForCombineTest()
    {
        const string testPath = "TestPath";
        const string testFile = "TestFile.dat";

        yield return new object[] { testPath, testFile, $"{testPath}{GenericPathHelper.GenericSeperator}{testFile}" };
        yield return new object[] { testFile, testPath, $"{testFile}{GenericPathHelper.GenericSeperator}{testPath}" };

        yield return new object[] { testPath + GenericPathHelper.GenericSeperator, testFile, $"{testPath}{GenericPathHelper.GenericSeperator}{testFile}" };

        yield return new object[] { string.Empty, string.Empty, string.Empty };
        yield return new object[] { string.Empty, testFile, testFile };
        yield return new object[] { testFile, string.Empty, testFile };
    }

    [Theory]
    [MemberData(nameof(GetTestNamesForCombineTest))]
    public void Test_Combine(string first, string secund, string expeced)
    {
        var result = GenericPathHelper.Combine(first, secund);

        result.Path.Should().Be(expeced);
    }

    [Fact]
    public void Test_NormalizePath()
    {
        const string toTest = "Test\\Test";
        const string expect = "Test/Test";

        var result = GenericPathHelper.NormalizePath(toTest);

        result.Path.Should().Be(expect);
    }

    [Fact]
    public void IsAbsolute_with_Absolute_Test()
    {
        const string toTest = "zip::C:/Test/test.zip";

        var result = GenericPathHelper.IsAbsolute(toTest);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAbsolute_with_Relative_Test()
    {
        const string toTest = "C:/Test/test.zip";

        var result = GenericPathHelper.IsAbsolute(toTest);

        result.Should().BeFalse();
    }

    [Fact]
    public void ToRelative_with_Absolute_Test()
    {
        const string toTest = "zip::C:/Test.zip";

        var relative = GenericPathHelper.ToRelativePath(toTest);

        relative.Path.Should().Be("C:/Test.zip");
        relative.Kind.Should().Be(PathType.Relative);
    }

    [Fact]
    public void ToRelative_with_Relative_Test()
    {
        const string toTest = "C:/Test.zip";
        
        var relative = GenericPathHelper.ToRelativePath(toTest);
        
        relative.Path.Should().Be(toTest);
        relative.Kind.Should().Be(PathType.Relative);
    }
}