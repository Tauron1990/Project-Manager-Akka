using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing;

public sealed record RegisterRule<TWorkspace, TData>(IRule<TWorkspace, TData> Rule)
    where TWorkspace : WorkspaceBase<TData> where TData : class;