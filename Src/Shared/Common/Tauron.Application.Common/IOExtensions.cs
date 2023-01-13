using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron;

[DebuggerStepThrough]
[PublicAPI]
public static class IoExtensions
{
    public static string PathShorten(this string path, int length)
    {
        string[] pathParts = path.Split('\\');
        var pathBuild = new StringBuilder(path.Length);
        string lastPart = pathParts[^1];
        var prevPath = "";

        //Erst prüfen ob der komplette String evtl. bereits kürzer als die Maximallänge ist
        if(path.Length >= length) return path;

        for (var i = 0; i < pathParts.Length - 1; i++)
        {
            pathBuild.Append(pathParts[i] + @"\");

            if((pathBuild + @"...\" + lastPart).Length >= length) return prevPath;

            prevPath = pathBuild + @"...\" + lastPart;
        }

        return prevPath;
    }

    public static IEnumerable<string> EnumerateTextLinesIfExis(this IFile file)
    {
        if(!file.Exist) yield break;

        using var reader = new StreamReader(file.Open(FileAccess.Read));

        while (true)
        {
            string? line = reader.ReadLine();

            if(line is null) yield break;

            yield return line;
        }
    }

    public static IEnumerable<string> EnumerateTextLines(this TextReader reader)
    {
        while (true)
        {
            string? line = reader.ReadLine();

            if(line == null) yield break;

            yield return line;
        }
    }

    public static void Clear(this IDirectory dic)
    {
        if(dic is null) throw new ArgumentNullException(nameof(dic));

        if(!dic.Exist) return;

        if(dic is IHasFileAttributes dicAttr)
            dicAttr.Attributes = FileAttributes.Normal;

        foreach (IFile file in dic.Files)
        {
            if(file is IHasFileAttributes fileAttributes)
                fileAttributes.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        foreach (IDirectory directory in dic.Directories)
        {
            Clear(directory);
            directory.Delete();
        }
    }

    public static TextWriter OpenWrite(this IFile file)
        => new StreamWriter(file.Open(FileAccess.Write));

    public static TextReader OpenRead(this IFile file)
        => new StreamReader(file.Open(FileAccess.Read));

    /*public static void AddFilesFromDictionary(this ZipArchive destination, string sourceDirectoryName)
    {
        var stack = new Stack<FileSystemInfo>();
        new DirectoryInfo(sourceDirectoryName).EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories)
           .Foreach(e => stack.Push(e));

        while (stack.Count != 0)
            switch (stack.Pop())
            {
                case FileInfo file:
                    destination.CreateEntryFromFile(
                        file.FullName,
                        file.FullName.Replace(sourceDirectoryName, string.Empty, StringComparison.Ordinal),
                        CompressionLevel.Optimal);

                    break;
                case DirectoryInfo directory:
                    string name = directory.FullName.Replace(sourceDirectoryName, string.Empty, StringComparison.Ordinal);
                    if (!string.IsNullOrWhiteSpace(name))
                        destination.CreateEntry(name);

                    break;
            }
    }



    public static string CombinePath(this string path, params string[] paths)
    {
        paths = paths.Select(str => str.TrimStart('\\')).ToArray();
        if (Path.HasExtension(path))
            path = Path.GetDirectoryName(path) ?? path;

        var tempPath = Path.Combine(paths);

        return Path.Combine(path, tempPath);
    }


    public static string CombinePath(this string? path, string path1)
        => string.IsNullOrWhiteSpace(path) ? path1 : Path.Combine(path, path1);

    public static string CombinePath(this FileSystemInfo path, string path1) => CombinePath(path.FullName, path1);

    public static void CopyFileTo(this string source, string destination)
    {
        if (!source.ExisFile()) return;

        File.Copy(source, destination, overwrite: true);
    }

    public static bool CreateDirectoryIfNotExis(this string path)
    {
        try
        {
            // ReSharper disable once InvertIf
            if (Path.HasExtension(path))
            {
                var temp = Path.GetDirectoryName(path);

                return CreateDirectoryIfNotExis(new DirectoryInfo(temp ?? throw new InvalidOperationException("No Valid File Path Parameter")));
            }

            return CreateDirectoryIfNotExis(new DirectoryInfo(path));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool CreateDirectoryIfNotExis(this DirectoryInfo dic)
    {
        if (dic.Exists) return false;

        dic.Create();

        return true;
    }

    public static void SafeDelete(this FileSystemInfo info)
    {
        if (info.Exists) info.Delete();
    }

    public static void DeleteDirectory(this string path)
    {
        if (Path.HasExtension(path))
            path = Path.GetDirectoryName(path) ?? path;

        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch (UnauthorizedAccessException) { }
    }

    public static void DeleteDirectory(this string path, object sub)
    {
        var tempsub = sub.ToString();

        if (string.IsNullOrWhiteSpace(tempsub))
            return;

        var compl = CombinePath(path, tempsub);
        if (Directory.Exists(compl)) Directory.Delete(compl);
    }

    public static void DeleteDirectory(this string path, bool recursive)
    {
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) new DirectoryInfo(path) { Attributes = FileAttributes.Normal }.Delete(recursive: true);
    }

    public static void DeleteDirectoryIfEmpty(this string path)
    {
        if (!Directory.Exists(path)) return;

        if (!Directory.EnumerateFileSystemEntries(path).Any()) Directory.Delete(path);
    }

    public static void DeleteFile(this string path)
    {
        if (!path.ExisFile()) return;

        File.Delete(path);
    }

    public static bool DirectoryConainsInvalidChars(this string path)
    {
        var invalid = Path.GetInvalidPathChars();

        return path.Any(invalid.Contains);
    }

    public static IEnumerable<string> EnumrateFileSystemEntries(this string dic)
        => Directory.EnumerateFileSystemEntries(dic);

    public static IEnumerable<string> EnumerateAllFiles(this string dic)
        => Directory.EnumerateFiles(dic, "*.*", SearchOption.AllDirectories);

    public static IEnumerable<string> EnumerateAllFiles(this string dic, string filter)
        => Directory.EnumerateFiles(dic, filter, SearchOption.AllDirectories);

    public static IEnumerable<string> EnumerateDirectorys(this string path) => Directory.Exists(path)
        ? Enumerable.Empty<string>()
        : Directory.EnumerateDirectories(path);

    public static IEnumerable<FileSystemInfo> EnumerateFileSystemEntrys(this string path)
        => new DirectoryInfo(path).EnumerateFileSystemInfos();

    public static IEnumerable<string> EnumerateFiles(this string dic)
        => Directory.EnumerateFiles(dic, "*.*", SearchOption.TopDirectoryOnly);

    public static IEnumerable<string> EnumerateFiles(this string dic, string filter)
        => Directory.EnumerateFiles(dic, filter, SearchOption.TopDirectoryOnly);





    public static bool ExisDirectory(this string path) => Directory.Exists(path);

    public static bool ExisFile(this string workingDirectory, string file)
    {
        try
        {
            return File.Exists(Path.Combine(workingDirectory, file));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool ExisFile(this string? file) => !string.IsNullOrWhiteSpace(file) && File.Exists(file);

    public static DateTime GetDirectoryCreationTime(this string path) => Directory.GetCreationTime(path);

    public static string? GetDirectoryName(this string path) => Path.GetDirectoryName(path);

    public static string? GetDirectoryName(this StringBuilder path) => GetDirectoryName(path.ToString());


    public static string[] GetDirectorys(this string path) => Directory.GetDirectories(path);

    public static string GetExtension(this string path) => Path.GetExtension(path);

    public static string GetFileName(this string path) => Path.GetFileName(path);

    public static string GetFileNameWithoutExtension(this string path) => Path.GetFileNameWithoutExtension(path);

    public static int GetFileSystemCount(this string strDir) => GetFileSystemCount(new DirectoryInfo(strDir));

    public static int GetFileSystemCount(this DirectoryInfo di)
    {
        var count = di.GetFiles().Length;

        // 2. Für alle Unterverzeichnisse im aktuellen Verzeichnis
        foreach (var diSub in di.GetDirectories())
        {
            // 2a. Statt Console.WriteLine hier die gewünschte Aktion
            count++;

            // 2b. Rekursiver Abstieg
            count += GetFileSystemCount(diSub);
        }

        return count;
    }

    public static string[] GetFiles(this string dic) => Directory.GetFiles(dic);

    public static string[] GetFiles(this string path, string pattern, SearchOption option)
        => Directory.GetFiles(path, pattern, option);

    public static string GetFullPath(this string path) => Path.GetFullPath(path);

    public static bool HasExtension(this string path) => Path.HasExtension(path);

    public static bool IsPathRooted(this string path) => Path.IsPathRooted(path);

    public static void MoveTo(this string source, string dest)
    {
        File.Move(source, dest);
    }

    public static void MoveTo(this string source, string workingDirectory, string dest)
    {
        var realDest = dest;

        if (!dest.HasExtension())
        {
            var fileName = Path.GetFileName(source);
            realDest = Path.Combine(dest, fileName);
        }

        var realSource = Path.Combine(workingDirectory, source);

        File.Move(realSource, realDest);
    }

    public static byte[] ReadAllBytesIfExis(this string path)
        => !File.Exists(path) ? Array.Empty<byte>() : File.ReadAllBytes(path);

    public static byte[] ReadAllBytes(this string path) => File.ReadAllBytes(path);

    public static string ReadTextIfExis(this string path)
        => File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    public static string ReadTextIfExis(this string workingDirectory, string subPath)
        => ReadTextIfExis(CombinePath(workingDirectory, subPath));

    public static IEnumerable<string> ReadTextLinesIfExis(this string path)
    {
        if (!File.Exists(path)) yield break;

        using var reader = File.OpenText(path);
        while (true)
        {
            var line = reader.ReadLine();

            if (line == null) break;

            yield return line;
        }
    }

    public static bool TryCreateUriWithoutScheme(this string str, [NotNullWhen(true)] out Uri? uri, params string[] scheme)
    {
        var creationOk = Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var target);

        // ReSharper disable once AccessToModifiedClosure
        if (creationOk)
            foreach (var s in scheme.Where(_ => creationOk))
                creationOk = target!.Scheme != s;

        uri = creationOk ? target : null;

        return creationOk;
    }

    public static void WriteTextContentTo(this string content, string path)
    {
        File.WriteAllText(path, content);
    }


    public static void WriteTextContentTo(this string content, string workingDirectory, string path)
    {
        WriteTextContentTo(content, CombinePath(workingDirectory, path));
    }

    public static Stream OpenRead(this string path, FileShare share)
    {
        path = path.GetFullPath();
        #pragma warning disable GU0011
        path.CreateDirectoryIfNotExis();

        return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, share);
    }

    public static Stream OpenRead(this string path) => OpenRead(path, FileShare.None);

    public static StreamWriter OpenTextAppend(this string path)
    {
        path.CreateDirectoryIfNotExis();

        return new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None));
    }

    public static StreamReader OpenTextRead(this string path) => File.OpenText(path);

    public static StreamWriter OpenTextWrite(this string path)
    {
        path.CreateDirectoryIfNotExis();

        return new StreamWriter(path);
    }

    public static Stream OpenWrite(this string path, bool delete = true) => OpenWrite(path, FileShare.None, delete);

    public static Stream OpenWrite(this string path, FileShare share, bool delete = true)
    {
        if (delete)
            path.DeleteFile();

        path = path.GetFullPath();
        path.CreateDirectoryIfNotExis();
        #pragma warning restore GU0011
        return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, share);
    }*/
}