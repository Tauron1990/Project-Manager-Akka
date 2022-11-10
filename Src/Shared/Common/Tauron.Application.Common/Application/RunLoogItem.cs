using System.Threading.Tasks;

namespace Tauron.Application;

public delegate ValueTask<Rollback<TContext>> RunLoogItem<TContext, in TItem>(Context<TContext> context, TItem item);