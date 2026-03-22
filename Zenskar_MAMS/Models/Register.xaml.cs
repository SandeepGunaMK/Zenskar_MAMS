using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class Register : Window
    {
        //private readonly DBContext _dbContext;        

        public Register()
        {
            InitializeComponent();
            //_dbContext = new DBContext();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Generate unique login ID
                string loginId = GenerateLoginId(TxtUserName.Text);

                #region OldQuery 
                // Insert new user
                //var parameters = new SqlParameter[]
                //{
                //    new("@loginId", loginId),
                //    new("@userName", TxtUserName.Text),
                //    new("@contactNumber", TxtContactNumber.Text),
                //    new("@password", TxtNewPassword.Password),
                //    new("@userType", RbMaster.IsChecked == true ? "Master" : "Instructor"),
                //    new("@status", "Pending Approval"),
                //    new("@createdDate", DateTime.Now)
                //};

                //string insertQuery = @"
                //    INSERT INTO User_Table (login_ID, User_Name, Contact_Number, Password, User_Type, Status, Created_Date)
                //    VALUES (@loginId, @userName, @contactNumber, @password, @userType, @status, @createdDate)";

                //_dbContext.InsertData(insertQuery, parameters);

                //// Create registration request
                //var requestParams = new SqlParameter[]
                //{
                //    new("@requestType", "Registration"),
                //    new("@requestedBy", TxtUserName.Text),
                //    new("@requestedDate", DateTime.Now)
                //};

                //string requestQuery = @"
                //    INSERT INTO Requests (RequestType, RequestedBy, Status, RequestedDate)
                //    VALUES (@requestType, @requestedBy, 'Open', @requestedDate)";

                //_dbContext.InsertData(requestQuery, requestParams);
                #endregion
                #region MongoDb
                // Get next User_ID
                int nextUserId = 1;

                var lastUser = CommonItems._mongoDBContext.Users
                    .Find(Builders<UserTable>.Filter.Empty)
                    .SortByDescending(u => u.User_ID)
                    .Limit(1)
                    .FirstOrDefault();

                if (lastUser != null)
                {
                    nextUserId = lastUser.User_ID + 1;
                }
                // Insert new user
                var user = new UserTable
                {
                    User_ID = nextUserId,
                    Login_ID = loginId,
                    User_Name = TxtUserName.Text,
                    Contact_Number = TxtContactNumber.Text,
                    Password = TxtNewPassword.Password,
                    User_Type = RbMaster.IsChecked == true ? "Master" : "Instructor",
                    Status = "Pending Approval",
                    Created_Date = DateTime.Now,
                    Approved_By = null,
                    Approved_Date = null
                };

                CommonItems._mongoDBContext.Users.InsertOne(user);
                int nextReqId = 1;

                var lastReq = CommonItems._mongoDBContext.Requests
                    .Find(Builders<RequestTable>.Filter.Empty)
                    .SortByDescending(r => r.Request_ID)
                    .Limit(1)
                    .Project(r => new RequestTable
                    {
                        Request_ID = r.Request_ID
                    })
                    .FirstOrDefault();

                if (lastReq != null)
                {
                    nextReqId = lastReq.Request_ID + 1;
                }
                // Create registration request
                var request = new RequestTable
                {
                    Request_ID = nextReqId,
                    RequestType = "Registration",
                    RequestedBy = TxtUserName.Text,
                    Status = "Open",
                    RequestedDate = DateTime.Now,
                    ApprovedBy = null,
                    ApprovedDate = null,
                    RejectedReason = null,
                    UpdatedData = null
                };

                CommonItems._mongoDBContext.Requests.InsertOne(request);
                #endregion


                MessageBox.Show($"Registration successful!\nYour Login ID is: {loginId}\n\nPlease wait for admin approval before logging in.",
                    "Registration Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtUserName.Text))
            {
                MessageBox.Show("Please enter a user name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(TxtContactNumber.Text, @"^\d{10}$"))
            {
                MessageBox.Show("Please enter a valid 10-digit contact number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TxtNewPassword.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TxtNewPassword.Password != TxtConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (RbMaster.IsChecked != true && RbInstructor.IsChecked != true)
            {
                MessageBox.Show("Please select a user type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        #region OldQuery 
        //private string GenerateLoginIdOld(string userName)
        //{
        //    // Remove spaces and special characters
        //    string baseName = Regex.Replace(userName, @"[^a-zA-Z]", "").ToUpper();

        //    // Take first 4 characters (or pad with 'X' if too short)
        //    baseName = (baseName + "XXXX").Substring(0, 4);

        //    // Get current max number for this base
        //    string query = $"SELECT MAX(login_ID) FROM User_Table WHERE login_ID LIKE '{baseName}%'";
        //    var result = _dbContext.SelectData(query);

        //    int nextNum = 1;
        //    if (result.Rows[0][0] != DBNull.Value)
        //    {
        //        string lastId = result.Rows[0][0].ToString();
        //        if (int.TryParse(lastId.Substring(4), out int lastNum))
        //        {
        //            nextNum = lastNum + 1;
        //        }
        //    }

        //    return $"{baseName}{nextNum:D3}";
        //}
        #endregion
        #region MongoDb
        private string GenerateLoginId(string userName)
        {
            string baseName = Regex.Replace(userName, @"[^a-zA-Z]", "")
                                   .ToUpper();

            baseName = (baseName + "XXXX").Substring(0, 4);

            var col = CommonItems.Db.GetCollection<BsonDocument>("Users");

            var filter = Builders<BsonDocument>.Filter.Regex(
                "Login_ID",
                new BsonRegularExpression($"^{baseName}[0-9]{{3}}$")
            );

            var lastDoc = col.Find(filter)
                             .Sort(Builders<BsonDocument>.Sort.Descending("Login_ID"))
                             .Limit(1)
                             .FirstOrDefault();

            int nextNum = 1;

            if (lastDoc != null)
            {
                string lastId = lastDoc["Login_ID"].AsString;
                nextNum = int.Parse(lastId.Substring(4)) + 1;
            }

            return $"{baseName}{nextNum:D3}";
        }
        #endregion
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}