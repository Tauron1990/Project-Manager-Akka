namespace Tauron.Application.Workshop.Analyzing.Rules;

public interface IRule<in TWorkspace, TData>
    where TWorkspace : WorkspaceBase<TData> where TData : class
{
    string Name { get; }

    void Init(object metadata, TWorkspace workspace);
}