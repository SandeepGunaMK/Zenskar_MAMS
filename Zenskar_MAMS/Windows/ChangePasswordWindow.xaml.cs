using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
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
                #region OldQuery 
                //var query = "SELECT Password FROM User_Table WHERE login_ID = @loginId";
                //var param = new SqlParameter[] { new SqlParameter("@loginId", loginId) };
                //var dt = _db.SelectData(query, param);
                #endregion
                #region MongoDb
                var filter = Builders<UserTable>.Filter.Eq(x => x.Login_ID, loginId);
                var projection = Builders<UserTable>.Projection.Include(x => x.Password);
                var dt = CommonItems._mongoDBContext.Users
                                    .Find(filter)
                                    .Project<UserTable>(projection)
                                    .ToList();


                //var userData = CommonItems._mongoDBContext.Users
                //                .Find(filter)
                //                .Project<UserTable>(projection)
                //                .ToList();
                #endregion
                if (dt == null || dt.Count == 0)
                {
                    ShowError("Invalid Login ID.");
                    return;
                }

                var storedPassword = dt[0].Password;
                if (storedPassword != currentPwd)
                {
                    ShowError("Current password is incorrect.");
                    return;
                }

                // Update password
                #region OldQuery                 
                //var updateQuery = "UPDATE User_Table SET Password = @newPwd WHERE login_ID = @loginId";
                //var updateParams = new SqlParameter[] {
                //    new SqlParameter("@newPwd", newPwd),
                //    new SqlParameter("@loginId", loginId)
                //};
                //var rowsAffected = _db.UpdateData(updateQuery, updateParams);
                #endregion
                #region MongoDb
                //var filter = Builders<UserTable>.Filter.Eq(x => x.Login_ID, loginId);
                var updates = Builders<UserTable>.Update.Set(x => x.Password, newPwd);
                var result = CommonItems._mongoDBContext.Users.UpdateOne(filter, updates);
                #endregion
                if (result.ModifiedCount > 0)
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
