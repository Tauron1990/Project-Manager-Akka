using System.Diagnostics;
using Tauron.Application.CommonUI;

namespace Akka.MGIHelper.UI.FanControl
{
    /// <summary>
    ///     Interaktionslogik für AutoFanControl.xaml
    /// </summary>
    public partial class AutoFanControl
    {
        public AutoFanControl(IViewModel<AutoFanControlModel> model)
            : base(model)
        {
            var trace = new StackTrace().ToString();

            InitializeComponent();
        }
    }
}