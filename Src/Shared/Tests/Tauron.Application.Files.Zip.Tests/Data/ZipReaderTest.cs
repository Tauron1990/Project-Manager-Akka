using System.IO;
using System.Text;
using FluentAssertions;
using Ionic.Zip;
using Tauron.Application.Files.Zip.Data;
using Xunit;

namespace Tauron.Application.Files.Zip.Tests.Data;

public class ZipReaderTest
{
    [Fact]
    // ReSharper disable once CognitiveComplexity
    public void TestZipReader()
    {
        var testDics = new[]
                       {
                           new[] { "Test/" },
                           new[] { "Test1", "Test2" },
                           new[] { "Test3", "Test4", "Test5" },
                           new[] { "Test6", "Test7", "Test8", "Test9" }
                       };

        var testfiles = new[]
                        {
                            new[] { "Test00.dat" },
                            new[] { "Test11", "Test2.dat" },
                            new[] { "Test3", "Test4", "Test5.dat" },
                            new[] { "Test666", "Test777", "Test888", "Test9.dat" }
                        };

        var testContent = Encoding.UTF8.GetBytes("Hallo Welt");

        using var stream = new MemoryStream();
        using var zipFile = new ZipFile();

        foreach (var dic in testDics) zipFile.AddEntry(new ZipEntry { FileName = string.Join('/', dic) });
        foreach (var files in testfiles) zipFile.AddEntry(string.Join('/', files), testContent);

        var result = ZipeReader.ReadData(zipFile);

        var toTest = result;
        foreach (var dic in testDics)
        {
            foreach (var pathElement in dic)
            {
                var targetElement = pathElement.Trim('/');
                
                var searchResult = toTest.Dics.TryGetValue(targetElement, out toTest);

                searchResult.Should().BeTrue();
                toTest!.Name.Should().Be(targetElement);
            }

            toTest = result;
        }

        toTest = result;
        
        foreach (var file in testfiles)
        {
            foreach (var pathElement in file)
            {
                var targetElement = pathElement.Trim('/');

                bool searchResult;

                if (Path.HasExtension(targetElement))
                {
                    searchResult = toTest.Files.TryGetValue(targetElement, out var testFile);

                    testFile!.Name.Should().Be(targetElement);
                }
                else
                {
                    searchResult = toTest.Dics.TryGetValue(targetElement, out toTest);
                    toTest!.Name.Should().Be(targetElement);
                }

                searchResult.Should().BeTrue();
            }

            toTest = result;
        }
    }
}