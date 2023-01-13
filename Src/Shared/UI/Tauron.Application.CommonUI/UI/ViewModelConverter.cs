namespace Tauron.Application.CommonUI.UI;

public class ViewModelConverter
{
    public object? Convert(object? value)
    {
        //if(!(parameter is IView root)) return value;
        if(value is not IViewModel model) return value;

        //var view = root.ViewManager.Get(model, root);
        //if (view != null)
        //    return view;

        AutoViewLocation manager = AutoViewLocation.Manager;
        var view = manager.ResolveView(model);

        return view.HasValue ? view.Value : value;

        //root.ViewManager.ThenRegister(model, view, root);
    }

    public object? ConvertBack(object? value)
        => value is not null ? ((dynamic)value).DataContext : null;
}