using JetBrains.Annotations;

namespace Tauron.Application.Wpf;

[PublicAPI]
public enum MsgBoxResult
{
    None = 0,
    Ok = 1,
    Cancel = 2,
    Yes = 6,
    No = 7,
}