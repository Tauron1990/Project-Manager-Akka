using System.Threading.Tasks;

namespace Tauron.Application;

public delegate ValueTask<Rollback<TContext>> RunStep<TContext>(Context<TContext> context);