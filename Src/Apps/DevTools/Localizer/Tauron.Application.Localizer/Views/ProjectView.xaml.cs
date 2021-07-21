using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Tauron.Application.CommonUI;
using Tauron.Application.Localizer.UIModels;
using Tauron.AkkaHost;
using Tauron.Localization;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ProjectView.xaml
    /// </summary>
    public partial class ProjectView
    {
        public ProjectView(IViewModel<ProjectViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }

        private void TextElement_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var myTextBox = (TextBox) sender;
            myTextBox.ContextMenu = new ContextMenu {Background = Brushes.DimGray};

            var caretIndex = myTextBox.CaretIndex;

            var cmdIndex = 0;
            var spellingError = myTextBox.GetSpellingError(caretIndex);
            if (spellingError == null) return;

            foreach (string str in spellingError.Suggestions)
            {
                MenuItem mi = new()
                {
                    Header = str,
                    FontWeight = FontWeights.Bold,
                    Command = EditingCommands.CorrectSpellingError,
                    CommandParameter = str,
                    CommandTarget = myTextBox
                };
                myTextBox.ContextMenu.Items.Insert(cmdIndex, mi);
                cmdIndex++;
            }

            Separator separatorMenuItem1 = new();
            myTextBox.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem1);
            cmdIndex++;
            MenuItem ignoreAllMi = new()
            {
                Header = ActorApplication.ActorSystem.Loc().Request("CorrectSpellingError").GetOrElse(string.Empty),
                Command = EditingCommands.IgnoreSpellingError,
                CommandTarget = myTextBox
            };
            myTextBox.ContextMenu.Items.Insert(cmdIndex, ignoreAllMi);
            cmdIndex++;
            Separator separatorMenuItem2 = new();
            myTextBox.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem2);
        }
    }
}