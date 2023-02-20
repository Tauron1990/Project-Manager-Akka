using System.Runtime.CompilerServices;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.Files.GridFS.Core;

internal static class ResolverRegister
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void RunInit() => ResolverRegistry.Register(new GridFsResolver());
}