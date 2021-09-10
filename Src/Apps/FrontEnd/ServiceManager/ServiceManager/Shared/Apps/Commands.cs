using System;
using Stl.CommandR;

namespace ServiceManager.Shared.Apps
{
    public sealed record RunAppSetupCommand(int Step) : ICommand<RunAppSetupResponse>
    {
        public static RunAppSetupCommand Initial()
            => new(0);
    }

    public sealed record RunAppSetupResponse(int NextStep, string? Error, bool IsCompled, string Message)
    {
        public RunAppSetupCommand CreateNextStep()
        {
            if (IsCompled)
                throw new InvalidOperationException("No next Steps");

            return string.IsNullOrWhiteSpace(Error) 
                       ?new RunAppSetupCommand(NextStep)
                       : throw new InvalidOperationException(Error);
        }
    }
}