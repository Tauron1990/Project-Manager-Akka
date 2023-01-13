using System.Threading.Tasks;

namespace Tauron.Application;

public delegate ValueTask Rollback<TContext>(Context<TContext> context);