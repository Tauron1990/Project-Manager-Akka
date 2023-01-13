using System;
using System.Threading.Tasks;

namespace Tauron.Application.Workshop.Mutation;

public interface IAsyncMutation : IDataMutation
{
    Func<Task> Run { get; }
}