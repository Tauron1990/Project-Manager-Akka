using Autofac;
using Tauron.Application.CommonUI;
using TimeTracker.ViewModels;

namespace TimeTracker
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>();

            base.Load(builder);
        }
    }
}