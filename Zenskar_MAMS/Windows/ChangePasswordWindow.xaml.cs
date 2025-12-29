using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace Zenskar_MAMS.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private readonly DBContext _db = new DBContext();

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            TxtError.Text = string.Empty;

            var loginId = TxtLoginId.Text?.Trim();
            var currentPwd = PwdCurrent.Password;
            var newPwd = PwdNew.Password;
            var confirmPwd = PwdConfirm.Password;

            // Client-side validation
            if (string.IsNullOrWhiteSpace(loginId))
            {
                ShowError("Login ID cannot be empty.");
                return;
            }
            if (string.IsNullOrEmpty(currentPwd))
            {
                ShowError("Current Password cannot be empty.");
                return;
            }
            if (string.IsNullOrEmpty(newPwd))
            {
                ShowError("New Password cannot be empty.");
                return;
            }
            if (newPwd != confirmPwd)
            {
                ShowError("New Password and Confirm Password do not match.");
                return;
            }

            try
            {
                // Fetch user record
                var query = "SELECT Password FROM User_Table WHERE login_ID = @loginId";
                var param = new SqlParameter[] { new SqlParameter("@loginId", loginId) };
                var dt = _db.SelectData(query, param);
                if (dt == null || dt.Rows.Count == 0)
                {
                    ShowError("Invalid Login ID.");
                    return;
                }

                var storedPassword = dt.Rows[0]["Password"]?.ToString();
                if (storedPassword != currentPwd)
                {
                    ShowError("Current password is incorrect.");
                    return;
                }

                // Update password
                var updateQuery = "UPDATE User_Table SET Password = @newPwd WHERE login_ID = @loginId";
                var updateParams = new SqlParameter[] {
                    new SqlParameter("@newPwd", newPwd),
                    new SqlParameter("@loginId", loginId)
                };

                var rowsAffected = _db.UpdateData(updateQuery, updateParams);
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Password updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    ShowError("Failed to update the password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"An error occurred: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
