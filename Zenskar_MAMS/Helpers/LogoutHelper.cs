using System.Windows;

namespace Zenskar_MAMS.Helpers
{
    class LogoutHelper
    {
        public static void Logout(Window currentWindow)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var loginWindow = new Windows.Login();
                loginWindow.Show();
                currentWindow.Close();
            }
        }
    }
}
