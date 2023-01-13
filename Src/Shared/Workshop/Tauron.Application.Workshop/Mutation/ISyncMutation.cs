using System;

namespace Tauron.Application.Workshop.Mutation;

public interface ISyncMutation : IDataMutation
{
    Action Run { get; }
}