using System;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.CommonUI
{
    public abstract class CommonUIFramework
    {
        public static bool NeedFullSync { get; set; }

        public abstract IUIApplication CreateDefault();

        public abstract IWindow CreateMessageDialog(string title, string message);

        public abstract object CreateDefaultMessageContent(
            string title, string message, Action<bool?>? result,
            bool canCnacel);
    }
}