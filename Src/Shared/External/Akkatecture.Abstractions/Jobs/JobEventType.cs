using JetBrains.Annotations;

namespace Akkatecture.Jobs;

[PublicAPI]
public enum JobEventType
{
    Cancel,
    Schedule,
    Finish,
}