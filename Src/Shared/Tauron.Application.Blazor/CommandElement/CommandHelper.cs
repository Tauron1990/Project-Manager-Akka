using JetBrains.Annotations;
using Stl.CommandR;

namespace Tauron.Application.Blazor.CommandElement;

public partial class CommandHelper
{
    [UsedImplicitly]
    public ICommand? Command { get; set; }

    [UsedImplicitly]
    public object? CommandParameter { get; set; }
}