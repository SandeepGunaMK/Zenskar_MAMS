using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class UserManagement : Window
    {
        private readonly DBContext _dbContext;

        public UserManagement()
        {
            InitializeComponent();
            _dbContext = new DBContext();

            CmbUserTypeFilter.SelectedIndex = 0;
            CmbStatusFilter.SelectedIndex = 0;

            LoadUsers();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AdminHome adminHome = new AdminHome();
            this.Close();
            adminHome.Show();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutHelper.Logout(this); OnClosed(e);
        }
        private void LoadUsers()
        {
            try
            {
                StringBuilder query = new StringBuilder("SELECT * FROM User_Table WHERE User_Type != 'Admin'");

                if (CmbUserTypeFilter.SelectedIndex > 0)
                {
                    query.Append(" AND User_Type = @userType");
                }

                if (CmbStatusFilter.SelectedIndex > 0)
                {
                    query.Append(" AND Status = @status");
                }

                query.Append(" ORDER BY Created_Date DESC");

                var parameters = new List<SqlParameter>();

                if (CmbUserTypeFilter.SelectedIndex > 0)
                {
                    parameters.Add(new SqlParameter("@userType", 
                        (CmbUserTypeFilter.SelectedItem as ComboBoxItem).Content.ToString()));
                }

                if (CmbStatusFilter.SelectedIndex > 0)
                {
                    parameters.Add(new SqlParameter("@status", 
                        (CmbStatusFilter.SelectedItem as ComboBoxItem).Content.ToString()));
                }

                var result = _dbContext.SelectData(query.ToString(), parameters.ToArray());
                UsersGrid.ItemsSource = result.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbUserTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUsers();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUsers();
        }

        private void UsersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewUserDetails();
        }

        private void MenuItemViewDetails_Click(object sender, RoutedEventArgs e)
        {
            ViewUserDetails();
        }

        private void ViewUserDetails()
        {
            if (UsersGrid.SelectedItem is DataRowView row)
            {
                var details = new StringBuilder();
                details.AppendLine($"User Details\n");
                details.AppendLine($"Login ID: {row["login_ID"]}");
                details.AppendLine($"Name: {row["User_Name"]}");
                details.AppendLine($"Contact Number: {row["Contact_Number"]}");
                details.AppendLine($"User Type: {row["User_Type"]}");
                details.AppendLine($"Status: {row["Status"]}");
                details.AppendLine($"Created Date: {Convert.ToDateTime(row["Created_Date"]).ToString("dd/MM/yyyy HH:mm")}");

                if (row["Approved_By"] != DBNull.Value)
                {
                    details.AppendLine($"Approved By: {row["Approved_By"]}");
                    details.AppendLine($"Approved Date: {Convert.ToDateTime(row["Approved_Date"]).ToString("dd/MM/yyyy HH:mm")}");
                }

                MessageBox.Show(details.ToString(), "User Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItemChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is DataRowView row)
            {
                var currentStatus = row["Status"].ToString();
                var userId = Convert.ToInt32(row["User_ID"]);

                var menu = new ContextMenu();
                foreach (var status in new[] { "Active", "Inactive" })
                {
                    if (status != currentStatus)
                    {
                        var menuItem = new MenuItem { Header = $"Set as {status}" };
                        menuItem.Click += (s, args) => ChangeUserStatus(userId, status);
                        menu.Items.Add(menuItem);
                    }
                }

                menu.IsOpen = true;
            }
        }
        #region OldQuery
        //private void ChangeUserStatus(int userId, string newStatus)
        //{
        //    try
        //    {
        //        var parameters = new SqlParameter[]
        //        {
        //            new("@userId", userId),
        //            new("@status", newStatus)
        //        };

        //        string query = "UPDATE User_Table SET Status = @status WHERE User_ID = @userId";
        //        _dbContext.UpdateData(query, parameters);

        //        MessageBox.Show("User status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //        LoadUsers();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error updating user status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        #endregion
        #region MongoDb
        private void ChangeUserStatus(int userId, string newStatus)
        {
            try
            {
                var filter = Builders<UserTable>.Filter.Eq(u => u.User_ID, userId);
                var update = Builders<UserTable>.Update.Set(u => u.Status, newStatus);

                var result = CommonItems._mongoContext.Users.UpdateOne(filter, update);

                if (result.MatchedCount == 0)
                {
                    MessageBox.Show("User not found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MessageBox.Show("User status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        private void MenuItemResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not DataRowView row)
                return;

            if (MessageBox.Show("Are you sure you want to reset this user's password?\nThe new password will be 'password123'", 
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    #region OldQuery
                    //var parameters = new SqlParameter[]
                    //{
                    //    new("@userId", Convert.ToInt32(row["User_ID"])),
                    //    new("@password", "password123")
                    //};
                    //string query = "UPDATE User_Table SET Password = @password WHERE User_ID = @userId";
                    //_dbContext.UpdateData(query, parameters);
                    #endregion
                    #region MongoDb
                    var userId = Convert.ToInt32(row["User_ID"]);
                    var filter = Builders<UserTable>.Filter.Eq(u => u.User_ID, userId);
                    var update = Builders<UserTable>.Update.Set(u => u.Password, "password123");
                    CommonItems._mongoContext.Users.UpdateOne(filter, update);
                    #endregion
                    MessageBox.Show("Password has been reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error resetting password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not DataRowView row)
                return;

            var userName = row["User_Name"].ToString();
            var userType = row["User_Type"].ToString();

            // Check if user has associated students
            try
            {
                #region OldQuery
                //var parameters = new SqlParameter[]
                //{
                //    new("@userName", userName)
                //};
                //string checkQuery = userType == "Master"
                //    ? "SELECT COUNT(*) FROM Student_Data WHERE MasterName = @userName"
                //    : "SELECT COUNT(*) FROM Student_Data WHERE InstructorName = @userName";
                //var result = _dbContext.SelectData(checkQuery, parameters);
                #endregion
                #region MongoDb
                var resList = userType == "Master" ? CommonItems._mongoContext.Students.CountDocuments(Builders<StudentTable>.Filter.Eq(s => s.MasterName, userName))
                : CommonItems._mongoContext.Students.CountDocuments(Builders<StudentTable>.Filter.Eq(s => s.InstructorName, userName));
                DataTable result = CommonItems.ToDataTableLong(resList);
                #endregion

                int studentCount = Convert.ToInt32(result.Rows[0][0]);

                if (studentCount > 0)
                {
                    MessageBox.Show($"Cannot delete user. They have {studentCount} students associated with them.", 
                        "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking student associations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Proceed with deletion if no students are associated
            if (MessageBox.Show($"Are you sure you want to delete user {userName}?\nThis action cannot be undone.", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var parameters = new SqlParameter[]
                    {
                        new("@userId", Convert.ToInt32(row["User_ID"]))
                    };

                    string deleteQuery = "DELETE FROM User_Table WHERE User_ID = @userId";
                    _dbContext.DeleteData(deleteQuery, parameters);

                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}