namespace SimpleProjectManager.Client.Data.Core;

public interface ISourceConfiguration<TActualState>
{
    IStateConfiguration<TActualState> FromServer()
}