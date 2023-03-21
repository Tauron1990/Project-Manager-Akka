using System.Threading.Tasks;

namespace Tauron.Application;

public delegate ValueTask<RunStepResult<TContext>> RunLoogItem<TContext, in TItem>(Context<TContext> context, TItem item);