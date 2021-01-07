using System;
using System.Reactive.Linq;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;

namespace Tauron.Application.CommonUI.UI
{
    public sealed class ViewConnector : ModelConnectorBase<ViewConnector>
    {
        private readonly IUIDispatcher _dispatcher;
        private readonly Action<object> _updater;
        
        public ViewConnector(string name, DataContextPromise promise, Action<object> updater, IUIDispatcher dispatcher)
            : base(name, promise)
        {
            _updater = updater;
            _dispatcher = dispatcher;
        }
        
        protected override void NoDataContextFound() => _updater($"No Data Context Found for {Name}");

        protected override void ValidateCompled(ValidatingEvent obj)
        {
        }

        protected override void PropertyChangedHandler(PropertyChangedEvent obj)
        {
            if(View == null) return;

            var converter = new ViewModelConverter();
            if (!(obj is {Value: IViewModel viewModel})) return;

            var viewTask = _dispatcher.InvokeAsync(() => converter.Convert(viewModel) as IView);

            viewTask.NotNull().Take(1).Subscribe(view => _updater(view));
        }

        public override string ToString() => "View Connector Loading...";
    }
}