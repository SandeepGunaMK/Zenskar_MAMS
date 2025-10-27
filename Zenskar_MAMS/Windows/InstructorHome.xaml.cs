using System;
using System.Windows;

namespace Zenskar_MAMS.Windows
{
    public partial class InstructorHome : Window
    {
        private readonly string _userName;

        public InstructorHome(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new ArgumentException("User name cannot be empty.", nameof(userName));
                }

                InitializeComponent();
                _userName = userName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing instructor home: {ex.Message}", 
                    "Initialization Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var studentsList = new StudentsList("Instructor", _userName);
                studentsList.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening students list: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            try
            {   
                this.Close();
                var requestsWindow = new RequestsWindow("Instructor", _userName);
                requestsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening requests window: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 1)
            {
                Application.Current.Shutdown();
            }
        }
    }
}