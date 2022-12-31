namespace Tauron.Application.Workshop.StateManagement.DataFactorys;

#pragma warning disable MA0048
public partial class MergeFactory
    #pragma warning restore MA0048
{
    public static AdvancedDataSourceFactory Merge(params AdvancedDataSourceFactory[] factories)
    {
        int foundFac = factories.FindIndex(a => a is MergeFactory);
        MergeFactory factory;

        if(foundFac != -1)
            factory = (MergeFactory)factories[foundFac];
        else
            factory = new MergeFactory();

        for (var i = 0; i < factories.Length; i++)
        {
            if(i == foundFac) continue;

            factory.Register(factories[i]);
        }

        return factory;
    }
}