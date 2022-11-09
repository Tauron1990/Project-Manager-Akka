using System.Collections.Concurrent;
using JetBrains.Annotations;
using SpaceConqueror.States;

namespace SpaceConqueror.Modules;

[PublicAPI]
public sealed class StateRegistrar
{
    private ConcurrentDictionary<Type, Func<IState>> _stateFactorys = new();

    internal StateRegistrar() { }

    public void Add<TType>()
        where TType : IState, new()
        => _stateFactorys[typeof(TType)] = static () => new TType();

    public void Add<TType>(Func<TType> factory)
        where TType : IState
        => _stateFactorys[typeof(TType)] = Wrap(factory);

    private static Func<IState> Wrap<TType>(Func<TType> factory)
        where TType : IState => () => factory();

    internal Func<IEnumerable<IState>> GetStates() => Wrap(_stateFactorys.Values);

    private static Func<IEnumerable<IState>> Wrap(ICollection<Func<IState>> states)
    {
        IEnumerable<IState> Run()
        {
            foreach (var state in states)
                yield return state();
        }

        return Run;
    }
}