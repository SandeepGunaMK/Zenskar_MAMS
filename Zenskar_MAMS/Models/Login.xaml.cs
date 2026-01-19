using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class Login : Window
    {
        private readonly DBContext _dbContext;

        public Login()
        {
            try
            {
                InitializeComponent();
                _dbContext = new DBContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing login window: {ex.Message}", 
                    "Initialization Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtLoginId.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
                {
                    MessageBox.Show("Please enter both login ID and password.", 
                        "Validation Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }
                #region OldQuery 
                //var parameters = new SqlParameter[]
                //{
                //    new("@loginId", TxtLoginId.Text),
                //    new("@password", TxtPassword.Password)
                //};

                //string query = @"
                //    SELECT User_ID, User_Name, User_Type, Status 
                //    FROM User_Table 
                //    WHERE login_ID = @loginId AND Password = @password";

                //DataTable result = _dbContext.SelectData(query, parameters);
                #endregion
                #region MongoDb
                var filter = Builders<UserTable>.Filter.Eq(x => x.login_ID, TxtLoginId.Text) & Builders<UserTable>.Filter.Eq(x => x.Password,TxtPassword.Password);
                var projection = Builders<UserTable>.Projection
                                 .Include(x => x.User_ID)
                                 .Include(x => x.User_Name)
                                 .Include(x => x.User_Type)
                                 .Include(x => x.Status)
                                 .Exclude("_id");                                 
                var col = common.Db.GetCollection<UserTable>("Users");
                var resList = col.Find(filter).Project<LoginUser1>(projection).ToList();
                DataTable result = common.ToDataTable(resList);
                #endregion



                if (result.Rows.Count == 0)
                {
                    MessageBox.Show("Invalid login credentials.", 
                        "Login Failed", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                var userRow = result.Rows[0];
                string status = userRow["Status"].ToString();
                string userName = userRow["User_Name"].ToString();
                
                if (status != "Active")
                {
                    MessageBox.Show($"Your account is currently {status}. Please contact administrator.", 
                        "Access Denied", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                Window homeWindow = new Window();
                string userType = userRow["User_Type"].ToString();

                switch (userType)
                {
                    case "Admin":
                        homeWindow = new AdminHome(userName);
                        break;
                    case "Master":
                        homeWindow = new MasterHome(userName);
                        break;
                    case "Instructor":
                        homeWindow = new InstructorHome(userName);
                        break;
                    default:
                        throw new Exception("Invalid user type");
                }

                if (homeWindow != null)
                {
                    homeWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var registerWindow = new Register();
                registerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening registration window: {ex.Message}", 
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