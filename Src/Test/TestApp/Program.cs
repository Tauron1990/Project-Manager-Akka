using System.IO;
using Ionic.Zip;

namespace TestApp
{
    static class Program
    {
        static void Main()
        {
            // const string testzip = @"D:\IMAGES\Daten Algemein\RemoveWindowsStoreApp.zip";
            //
            // var zip = ZipFile.Read(testzip);

            using var test = ZipFile.Read("test.zip");

            // using var zip = new ZipFile();
            //
            // zip.AddEntry("TestDic/Program.cs", File.Open("Program.cs", FileMode.Open));
            //
            // zip.Save("test.zip");
        }
    }
}