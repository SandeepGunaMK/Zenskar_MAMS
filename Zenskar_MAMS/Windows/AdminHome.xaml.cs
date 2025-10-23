using System.Windows;

namespace Zenskar_MAMS.Windows
{
    public partial class AdminHome : Window
    {
        private readonly string _userName;

        public AdminHome(string userName = "Admin")
        {
            InitializeComponent();
            _userName = userName;
        }

        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            var studentsList = new StudentsList("Admin", _userName);
            this.Close();
            studentsList.ShowDialog();
        }

        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            var requestsWindow = new RequestsWindow("Admin", _userName);
            this.Close();
            requestsWindow.ShowDialog();
        }

        private void BtnManageUsers_Click(object sender, RoutedEventArgs e)
        {
            var userManagement = new UserManagement();
            this.Close();
            userManagement.ShowDialog();
        }
    }
}