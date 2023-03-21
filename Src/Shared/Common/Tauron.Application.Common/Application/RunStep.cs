using System.Threading.Tasks;

namespace Tauron.Application;

public delegate ValueTask<RunStepResult<TContext>> RunStep<TContext>(Context<TContext> context);