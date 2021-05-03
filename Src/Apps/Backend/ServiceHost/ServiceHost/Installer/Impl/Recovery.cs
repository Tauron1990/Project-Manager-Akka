using System;
using System.Collections.Generic;
using Akka.Event;

namespace ServiceHost.Installer.Impl
{
    public sealed class Recovery
    {
        private readonly List<Action<ILoggingAdapter>> _entrys = new();

        public void Add(Action<ILoggingAdapter> recover)
            => _entrys.Add(recover);

        public void Recover(ILoggingAdapter logger)
        {
            foreach (var action in _entrys)
            {
                try
                {
                    action(logger);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error On Recover Action");
                }
            }
        }
    }
}