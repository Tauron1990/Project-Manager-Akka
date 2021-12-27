using System;
using System.IO;
using System.Runtime.CompilerServices;
using Ionic.Zip;

namespace Tauron.Application.Files.Zip.Data;

public static class ZipeReader
{
    public static ZipDirectoryContext ReadData(ZipFile file)
    {
        var start = new ZipDirectoryContext(file, null, "", null);

        foreach (var entry in file)
        {
            if (IsDirectory(entry))
                AddDirectory(file, entry, start);
            else
                AddFile(file, entry, start);
        }
        
        return start;
    }

    private static bool IsDirectory(ZipEntry entry)
    {
        if (entry.IsDirectory) return true;

        return !Path.HasExtension(entry.FileName);
    }
    
    #pragma warning disable EPS06
    private static void AddFile(ZipFile file, ZipEntry entry, ZipDirectoryContext root)
    {
        var fileName = entry.FileName.AsSpan();
        var nameIndex = fileName.LastIndexOf('/');

        if (nameIndex == -1)
            root.Files.TryAdd(entry.FileName, new ZipContext(file, entry, entry.FileName, root));
        else
        {
            var target = SearchDic(fileName[..nameIndex], file, root);
            var name = fileName[(nameIndex + 1)..].ToString();
            
            target.Files.TryAdd(name, new ZipContext(file, entry, name, target));
        }
    }
    
    private static void AddDirectory(ZipFile file, ZipEntry entry, ZipDirectoryContext root)
    {
        var target = SearchDic(entry.FileName.AsSpan(), file, root);

        if (target.Parent is null)
            throw new InvalidOperationException($"No Parent Found for Directory {entry.FileName}");

        target.Parent.Dics.TryUpdate(target.Name, target with { Entry = entry }, target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ZipDirectoryContext SearchDic(ReadOnlySpan<char> fileName, ZipFile file, ZipDirectoryContext dic)
    {
        var searchroom = fileName;
        if (searchroom[0] == '/')
            searchroom = searchroom[1..];
        
        do
        {
            var foundIndex = searchroom.IndexOf('/');
            if (foundIndex == -1)
            {
                return dic.Dics.AddOrUpdate(
                    searchroom.ToString(),
                    name => new ZipDirectoryContext(file, null, name, dic), 
                    (_, d) => d)!;
            }
            
            var parentDic = dic;
            
            dic = dic.Dics.AddOrUpdate(
                searchroom[..foundIndex].ToString(),
                name => new ZipDirectoryContext(file, null, name, parentDic), 
                (_, d) => d)!;

            searchroom = searchroom[(foundIndex + 1)..];

        } while (searchroom.Length != 0);

        return dic;
    }
    
    #pragma warning restore EPS06
}