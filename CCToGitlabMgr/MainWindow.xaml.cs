using System.Windows;
using System.Windows.Controls;
using CCToGitlabMgr.ViewModels;

namespace CCToGitlabMgr
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void ConsoleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.ScrollToEnd();
        }
    }
}
