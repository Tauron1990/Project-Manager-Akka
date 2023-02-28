using System.Diagnostics;
using Akka.Actor;
using Stl.Collections;

namespace Tauron.Features;

[DebuggerStepThrough]
public sealed record GenericState(Dictionary<Type, object> States)
{
    public static GenericState Create(IUntypedActorContext context, IEnumerable<IPreparedFeature> features)
    {
        var data = new Dictionary<Type, object>();
        data.AddRange(
            from feature in features
            let state = feature.InitialState(context)
            where state is not null
            select state.Value);
        
        return new(data);
    }

    public static GenericState Create(IUntypedActorContext context, IPreparedFeature feature)
    {
        var data = new Dictionary<Type, object>();
        
        TryAdd(context, data, feature);

        return new(data);
    }
    
    public static GenericState Create(IUntypedActorContext context, IPreparedFeature feature1, IPreparedFeature feature2)
    {
        var data = new Dictionary<Type, object>();
        
        TryAdd(context, data, feature1);
        TryAdd(context, data, feature2);
        
        return new(data);
    }
    
    public static GenericState Create(IUntypedActorContext context, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3)
    {
        var data = new Dictionary<Type, object>();
        
        TryAdd(context, data, feature1);
        TryAdd(context, data, feature2);
        TryAdd(context, data, feature3);

        return new(data);
    }

    public static GenericState Create(IUntypedActorContext context, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3, IPreparedFeature feature4)
    {
        var data = new Dictionary<Type, object>();
        
        TryAdd(context, data, feature1);
        TryAdd(context, data, feature2);
        TryAdd(context, data, feature3);
        TryAdd(context, data, feature4);
        
        return new(data);
    }
    
    private static void TryAdd(IUntypedActorContext context, ICollection<KeyValuePair<Type, object>> data, IPreparedFeature feature)
    {
        var value = feature.InitialState(context);
        if(value is null) return;
        
        data.Add(value.Value);
    }
}