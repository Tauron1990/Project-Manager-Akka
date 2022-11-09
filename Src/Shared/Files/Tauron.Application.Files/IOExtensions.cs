using System;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files;

[PublicAPI]
public static class IoExtensions
{
    public static void WriteAllBytes(this IFile file, byte[] bytes)
    {
        using Stream write = file.Open(FileAccess.Write);
        write.Write(bytes, 0, bytes.Length);
    }

    public static byte[] ReadAllBytes(this IFile file)
    {
        if(!file.Exist) return Array.Empty<byte>();

        using Stream stream = file.Open(FileAccess.Read);
        var arr = new byte[stream.Length];
        stream.Read(arr, 0, arr.Length);

        return arr;
    }
}