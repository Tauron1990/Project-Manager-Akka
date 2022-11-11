using System;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

namespace Tauron.Temp;

[PublicAPI]
public sealed class TempStorage : TempDic
{
    private static TempStorage? _default;

    public TempStorage()
        : this(DefaultNameProvider, new LocalSystem(Path.GetTempPath()), deleteBasePath: false) { }

    public TempStorage(Func<bool, string> nameProvider, IDirectory basePath, bool deleteBasePath)
        : base(basePath, default, nameProvider, deleteBasePath)
    {
        WireUp();
    }

    public static TempStorage Default => _default ??= new TempStorage();

    public static string DefaultNameProvider(bool file)
    {
        string name = Path.GetRandomFileName();

        return file ? name : name.Replace('.', '_');
    }

    public static TempStorage CleanAndCreate(IVirtualFileSystem path)
    {
        path.Clear();

        return new TempStorage(DefaultNameProvider, path, deleteBasePath: true);
    }

    private void WireUp()
        => AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

    private void OnProcessExit(object? sender, EventArgs e)
    {
        try
        {
            Dispose();
        }
        #pragma warning disable ERP022
        catch
        {
            //Ignored Due to Process Exit
        }
        #pragma warning restore ERP022
    }

    protected override void DisposeCore(bool disposing)
    {
        if(_default == this)
            _default = null;
        base.DisposeCore(disposing);
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
    }
}