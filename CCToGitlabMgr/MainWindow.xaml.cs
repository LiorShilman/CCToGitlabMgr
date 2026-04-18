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

        private void ProjectSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectSelector.SelectedItem is Services.ProjectInfo info)
            {
                var vm = DataContext as MainViewModel;
                vm?.LoadProjectCommand.Execute(info.ProjectId);
                // Reset selection so user can re-select the same project
                ProjectSelector.SelectedIndex = -1;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.OnWindowClosing(e);
        }
    }
}
