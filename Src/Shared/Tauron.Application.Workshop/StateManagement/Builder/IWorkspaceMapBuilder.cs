using System;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public interface IWorkspaceMapBuilder<TData> where TData : class
    {
        IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, IDataMutation> to);

        IWorkspaceMapBuilder<TData> MapAction<TAction>(Func<WorkspaceBase<TData>, TAction, IDataMutation> to);
    }
}