using System;

namespace Tauron.Application.CommonUI.AppCore
{
    public sealed class DelegateAppFactory : IAppFactory
    {
        private readonly Func<IUIApplication> _creator;

        public DelegateAppFactory(Func<IUIApplication> creator) => _creator = creator;

        public IUIApplication Create() => _creator();
    }
}