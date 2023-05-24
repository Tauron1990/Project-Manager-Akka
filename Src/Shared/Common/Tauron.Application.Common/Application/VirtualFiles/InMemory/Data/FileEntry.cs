using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reactive.PlatformServices;
using JetBrains.Annotations;
using Tauron.Errors;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public sealed class FileEntry : DataElementBase
{
    public MemoryStream? Data { get; private set; }

    public Result<string> ActualName => CheckNotInit().Bind(() => Result.Ok(Name));

    public Result<MemoryStream> ActualData => CheckNotInit().Bind(() => Result.Ok(Data))!;

    public Result<FileEntry> Init(string name, MemoryStream stream, ISystemClock clock)
    {
        if(string.IsNullOrWhiteSpace(name))
            return new NullOrEmpty(nameof(name));

        Name = name;
        Data = stream;
        ModifyDate = clock.UtcNow.LocalDateTime;

        return this;
    }

    public override void Dispose()
    {
        base.Dispose();

        Data?.Dispose();

        Data = null;
        Name = string.Empty;
    }

    private Result CheckNotInit()
        => Result.FailIf(
            Data is null || string.IsNullOrWhiteSpace(Name), 
            () =>new InvalidOperation().CausedBy("Entry not Initialized"));
}