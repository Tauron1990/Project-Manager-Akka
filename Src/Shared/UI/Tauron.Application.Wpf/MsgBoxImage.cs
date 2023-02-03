using JetBrains.Annotations;

namespace Tauron.Application.Wpf;

[PublicAPI]
public enum MsgBoxImage
{
    None = 0,
    Error = 16,

#pragma warning disable GU0060
    Hand = 16,
    Stop = 16,
    Question = 32,
    Exclamation = 48,
    Warning = 48,
    Asterisk = 64,
    Information = 64,
#pragma warning restore GU0060
}