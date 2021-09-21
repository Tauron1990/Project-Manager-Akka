using Syncfusion.SfSkinManager;

namespace SeriLogViewer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SfSkinManager.SetVisualStyle(this, VisualStyles.Blend);
            DataContext = new MainWindowModel(Grid);
        }
    }
}