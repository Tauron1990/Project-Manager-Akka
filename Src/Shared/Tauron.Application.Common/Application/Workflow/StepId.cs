using System.Diagnostics;
using JetBrains.Annotations;

namespace Tauron.Application.Workflow
{
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
        public override int GetHashCode() => Name.GetHashCode();

        public StepId(string name) : this()
            => Name = name;

        public string Name { get; }

        [DebuggerStepThrough]
        public override bool Equals(object? obj)
            => obj is StepId stepId && stepId.Name == Name;

        public static bool operator ==(StepId idLeft, StepId idRight) => idLeft.Name == idRight.Name;

        public static bool operator !=(StepId idLeft, StepId idRight) => idLeft.Name != idRight.Name;

        [DebuggerStepThrough]
        public override string ToString() => Name;
    }
}