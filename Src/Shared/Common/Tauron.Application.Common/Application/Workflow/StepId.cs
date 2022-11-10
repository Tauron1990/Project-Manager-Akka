using System.Diagnostics;
using JetBrains.Annotations;

namespace Tauron.Application.Workflow;

[PublicAPI]
public readonly struct StepId
{
    //public static readonly StepId Null = new StepId();

    public static readonly StepId Fail = new("Fail");
    public static readonly StepId None = new("None");
    public static readonly StepId Finish = new("Finish");
    public static readonly StepId Loop = new("Loop");
    public static readonly StepId LoopEnd = new("LoopEnd");
    public static readonly StepId LoopContinue = new("LoopContinue");
    public static readonly StepId Skip = new("Skip");
    public static readonly StepId Start = new("Start");
    public static readonly StepId Waiting = new("Waiting");

    [DebuggerStepThrough]
    public override int GetHashCode() => System.StringComparer.Ordinal.GetHashCode(Name);

    public StepId(string name) : this()
        => Name = name;

    public string Name { get; }

    [DebuggerStepThrough]
    public override bool Equals(object? obj)
        => obj is StepId stepId && string.Equals(stepId.Name, Name, System.StringComparison.Ordinal);

    public static bool operator ==(StepId idLeft, StepId idRight) => string.Equals(idLeft.Name, idRight.Name, System.StringComparison.Ordinal);

    public static bool operator !=(StepId idLeft, StepId idRight) => !string.Equals(idLeft.Name, idRight.Name, System.StringComparison.Ordinal);

    [DebuggerStepThrough]
    public override string ToString() => Name;
}