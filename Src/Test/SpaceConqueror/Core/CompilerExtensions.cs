using System.Linq.Expressions;
using FastExpressionCompiler;
using NRules;
using NRules.Extensibility;
using NRules.RuleModel;

namespace SpaceConqueror.Core;

public static class CompilerExtensions
{
    private sealed class FastExpressionCompiler : IExpressionCompiler
    {
        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression) where TDelegate : Delegate
            => expression.CompileFast();
    }

    public static ISessionFactory CompileFast(this IRuleRepository repository, IDependencyResolver dependencyResolver)
    {
        var compiler = new RuleCompiler
                       {
                           ExpressionCompiler = new FastExpressionCompiler()
                       };

        ISessionFactory? factory =  compiler.Compile(repository.GetRuleSets());
        factory.DependencyResolver = dependencyResolver;

        return factory;
    }
}