namespace DatabaseScaffold.Views
{
    using DatabaseScaffold.ViewModels;
    using MahApps.Metro.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        public ShellView(ShellViewModel dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
