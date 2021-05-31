using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.CommonUI.Dialogs;
using TimeTracker.Data;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaktionslogik für CorrectionDialog.xaml
    /// </summary>
    public partial class CorrectionDialog : IBaseDialog<ProfileEntry, ProfileEntry>
    {
        public CorrectionDialog()
        {
            InitializeComponent();
        }

        public Task<ProfileEntry> Init(ProfileEntry initalData) => throw new NotImplementedException();
    }
}
