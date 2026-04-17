using System.Windows.Controls;
using System.Windows.Input;
using CCToGitlabMgr.Models;

namespace CCToGitlabMgr.Views
{
    public partial class ChecklistStepView : UserControl
    {
        public ChecklistStepView() { InitializeComponent(); }

        private void ChecklistItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.FrameworkElement fe && fe.DataContext is ChecklistItem item)
            {
                item.IsDone = !item.IsDone;
            }
        }
    }
}
