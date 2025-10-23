using System.Windows;

namespace Zenskar_MAMS.Windows
{
    public partial class MasterHome : Window
    {
        private readonly string _userName;

        public MasterHome(string userName)
        {
            InitializeComponent();
            _userName = userName;
        }

        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            var studentsList = new StudentsList("Master", _userName);
            studentsList.ShowDialog();
        }

        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            var requestsWindow = new RequestsWindow("Master", _userName);
            requestsWindow.ShowDialog();
        }
    }
}