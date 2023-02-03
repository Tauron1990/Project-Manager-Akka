using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Resources;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Implementation;

[PublicAPI]
public class PackUriHelper : IPackUriHelper
{
    public string GetString(string pack) => GetString(pack, Assembly.GetCallingAssembly().GetName().Name, full: false);

    public string GetString(string pack, string? assembly, bool full)
    {
        if(assembly is null) return pack;

        string fullstring = full ? "pack://application:,,," : string.Empty;

        return $"{fullstring}/{assembly};component/{pack}";
    }

    public Uri GetUri(string pack) => GetUri(pack, Assembly.GetCallingAssembly().GetName().Name, full: false);

    public Uri GetUri(string pack, string? assembly, bool full)
    {
        string compledpack = GetString(pack, assembly, full);
        UriKind uriKind = full ? UriKind.Absolute : UriKind.Relative;

        return new Uri(compledpack, uriKind);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public T Load<T>(string pack) where T : class => Load<T>(pack, Assembly.GetCallingAssembly().GetName().Name);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public T Load<T>(string pack, string? assembly) where T : class
        => (T)System.Windows.Application.LoadComponent(GetUri(pack, assembly, full: false));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Stream LoadStream(string pack) => LoadStream(pack, Assembly.GetCallingAssembly().GetName().Name);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Stream LoadStream(string pack, string? assembly)
    {
        StreamResourceInfo? info = System.Windows.Application.GetResourceStream(GetUri(pack, assembly, full: true));

        if(info != null) return info.Stream;

        throw new InvalidOperationException("Stream loading Failed");
    }
}