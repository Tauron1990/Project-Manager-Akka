using System.Runtime.CompilerServices;
using VerifyTests;

namespace Tauron.SourceGenerators.Tests;

public static class Initializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}