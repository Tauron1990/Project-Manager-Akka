namespace SpaceConqueror.Modules;

public sealed class RuleRegistrar
{
    private readonly HashSet<Type> _rules = new();

    public void Register(Type type)
        => _rules.Add(type);

    public void Register<TType>()
        => _rules.Add(typeof(TType));

    public IEnumerable<Type> GetRules() => _rules.AsEnumerable();
}