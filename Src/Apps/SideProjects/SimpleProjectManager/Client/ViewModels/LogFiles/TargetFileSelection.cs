using System.Reactive;
using OneOf;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

[GenerateOneOf]
public partial class TargetFileSelection : OneOfBase<TargetFile, Unit>
{
    public static TargetFileSelection NoFile => new(default(Unit));
}