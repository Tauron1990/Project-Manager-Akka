using System.Runtime.CompilerServices;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.Files.GridFS.Core;

internal static class ResolverRegister
{
    [ModuleInitializer]
    internal static void RunInit() => ResolverRegistry.Register(new GridFsResolver());
}