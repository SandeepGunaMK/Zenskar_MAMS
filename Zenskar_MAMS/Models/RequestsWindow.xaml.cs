using Azure.Core;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System.Linq;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class RequestsWindow : Window
    {
        //private readonly DBContext _dbContext;
        private readonly string _currentUserType;
        private readonly string _currentUserName;
        private static readonly RoutedCommand ApproveCommand = new RoutedCommand();

        public RequestsWindow(string userType, string userName)
        {
            InitializeComponent();
            //_dbContext = new DBContext();
            _currentUserType = userType;
            _currentUserName = userName;

            // Command binding for the approve/view menu item
            CommandBinding commandBinding = new CommandBinding(
                ApproveCommand,
                MenuItemApprove_Click
            );
            this.CommandBindings.Add(commandBinding);

            LoadRequests();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutHelper.Logout(this); OnClosed(e);
        }
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 1)
            {
                Application.Current.Shutdown();
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentUserType)
            {
                case "Admin":
                    AdminHome adminHome = new AdminHome(_currentUserName);
                    this.Close();
                    adminHome.Show();
                    break;
                case "Master":
                    MasterHome masterHome = new MasterHome(_currentUserName);
                    this.Close();
                    masterHome.Show();
                    break;
                case "Instructor":
                    InstructorHome instructorHome = new InstructorHome(_currentUserName);
                    this.Close();
                    instructorHome.Show();
                    break;
            }
        }
        private void ConfigurePermissions()
        {
            switch (_currentUserType)
            {
                case "Admin":
                    // Admin can see and approve/reject all requests
                    break;
                case "Master":
                    // Masters can only approve/reject Update requests
                    MenuItemApprove.IsEnabled = true;
                    MenuItemReject.IsEnabled = true;
                    break;
                case "Instructor":
                    // Instructors can only view requests
                    MenuItemApprove.Visibility = Visibility.Collapsed;
                    MenuItemReject.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadRequests()
        {
            #region OldQuery
            //try
            //{
            //    StringBuilder query = new StringBuilder(@"
            //        SELECT R.*, 
            //               CASE 
            //                   WHEN R.Student_ID IS NOT NULL THEN S.Name 
            //                   ELSE NULL 
            //               END AS StudentName
            //        FROM Requests R
            //        LEFT JOIN Student_Data S ON R.Student_ID = S.Student_ID
            //        WHERE 1=1");

            //    var conditions = new List<string>();
            //    var parameters = new List<SqlParameter>();

            //    // Status filters
            //    var statusFilters = new List<string>();
            //    if (ChkOpen.IsChecked == true) statusFilters.Add("'Open'");
            //    if (ChkApproved.IsChecked == true) statusFilters.Add("'Approved'");
            //    if (ChkRejected.IsChecked == true) statusFilters.Add("'Rejected'");

            //    if (statusFilters.Count > 0)
            //    {
            //        conditions.Add($"R.Status IN ({string.Join(",", statusFilters)})");
            //    }

            //    // Type filters
            //    var typeFilters = new List<string>();
            //    if (ChkRegistration.IsChecked == true) typeFilters.Add("'Registration'");
            //    if (ChkUpdate.IsChecked == true) typeFilters.Add("'Update'");
            //    if (ChkDelete.IsChecked == true) typeFilters.Add("'Delete'");

            //    if (typeFilters.Count > 0)
            //    {
            //        conditions.Add($"R.RequestType IN ({string.Join(",", typeFilters)})");
            //    }

            //    // For non-admin users, show only relevant requests
            //    if (_currentUserType != "Admin")
            //    {
            //        if (_currentUserType == "Master")
            //        {
            //            conditions.Add("(R.RequestType = 'Update' OR R.RequestedBy = @userName)");
            //            parameters.Add(new SqlParameter("@userName", _currentUserName));
            //        }
            //        else // Instructor
            //        {
            //            conditions.Add("R.RequestedBy = @userName");
            //            parameters.Add(new SqlParameter("@userName", _currentUserName));
            //        }
            //    }

            //    if (conditions.Count > 0)
            //    {
            //        query.Append(" AND ").Append(string.Join(" AND ", conditions));
            //    }

            //    query.Append(" ORDER BY R.RequestedDate DESC");

            //    var result = _dbContext.SelectData(query.ToString(), parameters.ToArray());
            //    RequestsGrid.ItemsSource = result.DefaultView;
            //}
            #endregion
            #region MongoDB
            try
            {
                var filterBuilder = Builders<RequestTable>.Filter;
                var filters = new List<FilterDefinition<RequestTable>>();

                // ----------------------
                // Status filters
                // ----------------------
                var statusFilters = new List<string>();
                if (ChkOpen.IsChecked == true) statusFilters.Add("Open");
                if (ChkApproved.IsChecked == true) statusFilters.Add("Approved");
                if (ChkRejected.IsChecked == true) statusFilters.Add("Rejected");

                if (statusFilters.Count > 0)
                    filters.Add(filterBuilder.In(x => x.Status, statusFilters));

                // ----------------------
                // Type filters
                // ----------------------
                var typeFilters = new List<string>();
                if (ChkRegistration.IsChecked == true) typeFilters.Add("Registration");
                if (ChkUpdate.IsChecked == true) typeFilters.Add("Update");
                if (ChkDelete.IsChecked == true) typeFilters.Add("Delete");

                if (typeFilters.Count > 0)
                    filters.Add(filterBuilder.In(x => x.RequestType, typeFilters));

                // ----------------------
                // Role-based filters
                // ----------------------
                if (_currentUserType != "Admin")
                {
                    if (_currentUserType == "Master")
                    {
                        var masterFilter = filterBuilder.Or(
                            filterBuilder.Eq(x => x.RequestType, "Update"),
                            filterBuilder.Eq(x => x.RequestedBy, _currentUserName)
                        );

                        filters.Add(masterFilter);
                    }
                    else // Instructor
                    {
                        filters.Add(filterBuilder.Eq(x => x.RequestedBy, _currentUserName));
                    }
                }

                var finalFilter = filters.Count > 0
                    ? filterBuilder.And(filters)
                    : FilterDefinition<RequestTable>.Empty;

                // Fetch full RequestTable documents so UpdatedData can be inspected as BsonValue
                var requestData = CommonItems._mongoDBContext.Requests
                                    .Find(finalFilter)
                                    .SortByDescending(x => x.RequestedDate)
                                    .ToList();

                //----------------------
                //LEFT JOIN + CASE logic
                //----------------------
                var studentIds = requestData
                                    .Where(x => x.Student_ID.HasValue)
                                    .Select(x => x.Student_ID)
                                    .Distinct()
                                    .ToList(); // List<int?>

                var studentProjection = Builders<StudentTable>.Projection.Exclude("_id");
                var studentFilter = Builders<StudentTable>.Filter.In(x => x.Student_ID, studentIds);

                var studentData = CommonItems._mongoDBContext.Students
                                    .Find(studentFilter)
                                    .Project<StudentTable>(studentProjection)
                                    .ToList();

                var studentDictionary = studentData
                                            .Where(x => x.Student_ID.HasValue)
                                            .ToDictionary(x => x.Student_ID!.Value, x => x.Name);

                // Apply CASE logic equivalent and project a flat object matching the DataGrid bindings
                //var finalResult = requestData.Select(r => new
                //{
                //    r.Request_ID,
                //    r.RequestType,
                //    r.RequestedBy,
                //    StudentName = (r.Student_ID.HasValue && studentDictionary.ContainsKey(r.Student_ID.Value))
                //                    ? studentDictionary[r.Student_ID.Value]
                //                    : null,
                //    Student_ID = r.Student_ID,
                //    r.Status,
                //    r.RejectedReason,
                //    r.RequestedDate,
                //    r.ApprovedBy,
                //    r.ApprovedDate,
                //    // Convert UpdatedData (BsonValue) to a JSON string if it's a document/array, otherwise ToString()
                //    UpdatedData = r.UpdatedData == null ? null : (r.UpdatedData.IsBsonDocument || r.UpdatedData.IsBsonArray ? r.UpdatedData.ToJson() : r.UpdatedData.ToString())
                //}).ToList();
                var finalResult = requestData.Select(r => new RequestGridModel
                {
                    Request_ID = r.Request_ID,
                    RequestType = r.RequestType,
                    RequestedBy = r.RequestedBy,
                    StudentName = (r.Student_ID.HasValue && studentDictionary.ContainsKey(r.Student_ID.Value))
                                    ? studentDictionary[r.Student_ID.Value]
                                    : null,
                    Student_ID = r.Student_ID,
                    Status = r.Status,
                    RejectedReason = r.RejectedReason,
                    RequestedDate = r.RequestedDate,
                    ApprovedBy = r.ApprovedBy,
                    ApprovedDate = r.ApprovedDate,
                    UpdatedData = r.UpdatedData?.ToJson()
                }).ToList();

                RequestsGrid.ItemsSource = finalResult;
            }
            #endregion
            catch (Exception ex)
            {
                //MessageBox.Show($"Error loading requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void RequestsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewRequestDetails();
        }

        private void MenuItemViewDetails_Click(object sender, RoutedEventArgs e)
        {
            ViewRequestDetails();
        }

        private void ViewRequestDetails()
        {
            if (RequestsGrid.SelectedItem is RequestGridModel row)
            {
                //string requestType = row.RequestType.ToString();
                string requestType = row.RequestType.ToString();
                if (requestType == "Registration")
                {
                    ShowRegistrationDetails(row);
                }
                //else if (int.TryParse(row.Student_ID?.ToString(), out int studentId))
                else if (int.TryParse(row.Student_ID?.ToString(), out int studentId))
                {
                    var studentDetails = new StudentDetails(studentId, _currentUserType, _currentUserName);
                    studentDetails.ShowDialog();
                }
            }
        }

        private void ShowRegistrationDetails(RequestGridModel request)
        {
            try
            {
                #region OldQuery
                //var parameters = new SqlParameter[]
                //{
                //    new("@userName", request["RequestedBy"].ToString())
                //};

                //string query = "SELECT * FROM User_Table WHERE User_Name = @userName";
                //var result = _dbContext.SelectData(query, parameters);

                //if (result.Rows.Count > 0)
                //{
                //    var details = result.Rows[0];
                //    string message = $"Registration Details:\n\n" +
                //                   $"Name: {details["User_Name"]}\n" +
                //                   $"Login ID: {details["login_ID"]}\n" +
                //                   $"Contact: {details["Contact_Number"]}\n" +
                //                   $"User Type: {details["User_Type"]}\n" +
                //                   $"Created Date: {details["Created_Date"]}";

                //    MessageBox.Show(message, "Registration Details", MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                #endregion
                #region MongoDBQuery
                //var userName = request["RequestedBy"]?.ToString();
                var userName = request.RequestedBy?.ToString();
                var projection = Builders<UserTable>.Projection.Exclude("_id");
                var filter = Builders<UserTable>.Filter.Eq(x => x.User_Name, userName);
                var userData = CommonItems._mongoDBContext.Users
                                .Find(filter)
                                .Project<UserTable>(projection)
                                .ToList();

                if (userData.Count > 0)
                {
                    var details = userData[0];
                    string message = $"Registration Details:\n\n" +
                                     $"Name: {details.User_Name}\n" +
                                     $"Login ID: {details.Login_ID}\n" +
                                     $"Contact: {details.Contact_Number}\n" +
                                     $"User Type: {details.User_Type}\n" +
                                     $"Created Date: {details.Created_Date}";

                    MessageBox.Show(message, "Registration Details",MessageBoxButton.OK, MessageBoxImage.Information);

                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading registration details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MenuItemApprove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanApproveRequest())
                return;

            if (RequestsGrid.SelectedItem is RequestGridModel row)
            {
                try
                {
                    if (row.RequestType.ToString() == "Update")
                    {
                        // Get current student data
                        var studentId = Convert.ToInt32(row.Student_ID);

                        #region OldQuery                        
                        //var parameters = new SqlParameter[] { new("@studentId", studentId) };
                        //string query = "SELECT * FROM Student_Data WHERE Student_ID = @studentId";
                        //var result = _dbContext.SelectData(query, parameters);
                        #endregion
                        #region MongoDBQuery
                        var projection = Builders<StudentTable>.Projection.Exclude("_id");
                        var filter = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, studentId);
                        var resultMongo = CommonItems._mongoDBContext.Students
                                        .Find(filter)
                                        .Project<StudentTable>(projection)
                                        .ToList();
                        var result = CommonItems.ToDataTable(resultMongo);
                        #endregion

                        if (result.Rows.Count > 0)
                        {
                            var currentValues = new Dictionary<string, object>();
                            foreach (DataColumn col in result.Columns)
                            {
                                currentValues[col.ColumnName] = result.Rows[0][col];
                            }

                            // Show comparison window
                            var detailsWindow = new RequestDetailsWindow(
                                currentValues,
                                row.UpdatedData.ToString(),
                                () => ProcessApproval(row),
                                () => ShowRejectDialog(row)
                            );
                            detailsWindow.Owner = this;
                            LoadRequests();
                            detailsWindow.ShowDialog();
                        }
                    }
                    else if (MessageBox.Show("Are you sure you want to approve this request?", 
                        "Confirm Approval", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        ProcessApproval(row);
                        LoadRequests();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing request: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }       
        private void ShowRejectDialog(RequestGridModel row)
        {
            var reasonWindow = new RejectReasonWindow();
            if (reasonWindow.ShowDialog() == true)
            {
                try
                {
                    #region OldQuery
                    //var parameters = new SqlParameter[]
                    //{
                    //    new("@requestId", Convert.ToInt32(row.Request_ID)),
                    //    new("@rejectedReason", reasonWindow.Reason),
                    //    new("@approvedBy", _currentUserName),
                    //    new("@approvedDate", DateTime.Now)
                    //};

                    //string query = @"
                    //    UPDATE Requests 
                    //    SET Status = 'Rejected',
                    //        RejectedReason = @rejectedReason,
                    //        ApprovedBy = @approvedBy,
                    //        ApprovedDate = @approvedDate
                    //    WHERE Request_ID = @requestId";

                    //_dbContext.UpdateData(query, parameters);
                    #endregion
                    #region MongoDB
                    var filter = Builders<RequestTable>.Filter.Eq(x => x.Request_ID, Convert.ToInt32(row.Request_ID));
                    var update = Builders<RequestTable>.Update
                                .Set(x => x.Status, "Rejected")
                                .Set(x => x.RejectedReason, reasonWindow.Reason)
                                .Set(x => x.ApprovedBy, _currentUserName)
                                .Set(x => x.ApprovedDate, DateTime.Now);
                    CommonItems._mongoDBContext.Requests.UpdateOne(filter, update);
                    #endregion
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error rejecting request: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanApproveRequest()
        {
            if (RequestsGrid.SelectedItem is not RequestGridModel row)
            {
                MessageBox.Show("Please select a request to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string requestType = row.RequestType.ToString();
            string status = row.Status.ToString();

            if (status != "Open")
            {
                MessageBox.Show("Only open requests can be approved.", "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_currentUserType == "Master" && requestType != "Update")
            {
                MessageBox.Show("Masters can only approve Update requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ProcessApproval(RequestGridModel row)
        {
            string requestType = row.RequestType.ToString();
            int requestId = Convert.ToInt32(row.Request_ID);

            switch (requestType)
            {
                case "Registration":
                    ApproveRegistration(row.RequestedBy.ToString());
                    break;
                case "Update":
                    if (int.TryParse(row.Student_ID?.ToString(), out int studentId))
                    {
                        string updatedDataJson = row.UpdatedData?.ToString();
                        if (!string.IsNullOrEmpty(updatedDataJson))
                        {
                            try
                            {
                                var updatedData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(updatedDataJson);
                                UpdateStudent(studentId, updatedData);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error processing update data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    break;
                case "Delete":
                    if (int.TryParse(row.Student_ID?.ToString(), out studentId))
                    {
                        DeleteStudent(studentId);
                    }
                    break;
            }

            // Update request status
            #region OldQuery   
            //var parameters = new SqlParameter[]
            //{
            //    new("@requestId", requestId),
            //    new("@approvedBy", _currentUserName),
            //    new("@approvedDate", DateTime.Now)
            //};

            //string updateQuery = @"
            //    UPDATE Requests 
            //    SET Status = 'Approved',
            //        ApprovedBy = @approvedBy,
            //        ApprovedDate = @approvedDate
            //    WHERE Request_ID = @requestId";

            //_dbContext.UpdateData(updateQuery, parameters);
            #endregion
            #region MongoDBQuery
            var filter = Builders<RequestTable>.Filter.Eq(x => x.Request_ID, requestId);
            var update = Builders<RequestTable>.Update
                .Set(x => x.Status, "Approved")
                .Set(x => x.ApprovedBy, _currentUserName)
                .Set(x => x.ApprovedDate, DateTime.Now);
            CommonItems._mongoDBContext.Requests.UpdateOne(filter, update);
            LoadRequests();
            #endregion
        }

        private void ApproveRegistration(string userName)
        {
            #region OldQuery            
            //var parameters = new SqlParameter[]
            //{
            //    new("@userName", userName),
            //    new("@approvedBy", _currentUserName),
            //    new("@approvedDate", DateTime.Now)
            //};

            //string query = @"
            //    UPDATE User_Table 
            //    SET Status = 'Active',
            //        Approved_By = @approvedBy,
            //        Approved_Date = @approvedDate
            //    WHERE User_Name = @userName";

            //_dbContext.UpdateData(query, parameters);
            #endregion
            #region MongoDBQuery
            var filter = Builders<UserTable>.Filter.Eq(x => x.User_Name, userName);
            var update = Builders<UserTable>.Update
                .Set(x => x.Status, "Active")
                .Set(x => x.Approved_By, _currentUserName)
                .Set(x => x.Approved_Date, DateTime.Now);
            CommonItems._mongoDBContext.Users.UpdateOne(filter, update);
            LoadRequests();
            #endregion
        }

        #region OldQuery
        /*
        private void UpdateStudent(int studentId, Dictionary<string, object> updatedData)
        {
            var parameters = new List<SqlParameter>
            {
                new("@studentId", studentId)
            };

            var updateParts = new List<string>();
            foreach (var kvp in updatedData)
            {
                string paramName = $"@{kvp.Key}";
                object value = kvp.Value;

                // Handle special cases
                if (value is JsonElement element)
                {
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.String:
                            value = element.GetString();
                            break;
                        case JsonValueKind.Number:
                            value = element.GetInt32();
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            value = element.GetBoolean();
                            break;
                        case JsonValueKind.Null:
                            value = DBNull.Value;
                            break;
                        default:
                            continue;
                    }
                }

                parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
                updateParts.Add($"{kvp.Key} = {paramName}");
            }
            //TobeChanged

            string updateQuery = $@"
                UPDATE Student_Data 
                SET {string.Join(", ", updateParts)}
                WHERE Student_ID = @studentId";

            try
            {
                _dbContext.UpdateData(updateQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating student data: {ex.Message}");
            }
        }
        */
        #endregion
        #region MongoDB
        private void UpdateStudent(int studentId, Dictionary<string, object> updatedData)
        {
            try
            {
                var updater = Builders<StudentTable>.Update;
                var updateDefs = new List<UpdateDefinition<StudentTable>>();

                foreach (var kvp in updatedData)
                {
                    object? value = kvp.Value;

                    if (value is JsonElement element)
                    {
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.String:
                                var s = element.GetString();
                                // Try parse as datetime, otherwise keep string
                                if (DateTime.TryParse(s, out var dt))
                                    value = dt;
                                else
                                    value = s;
                                break;
                            case JsonValueKind.Number:
                                if (element.TryGetInt32(out var i))
                                    value = i;
                                else if (element.TryGetInt64(out var l))
                                    value = l;
                                else if (element.TryGetDouble(out var d))
                                    value = d;
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                value = element.GetBoolean();
                                break;
                            case JsonValueKind.Null:
                                value = BsonNull.Value;
                                break;
                            case JsonValueKind.Object:
                                // Handle MongoDB extended JSON date wrapper: { "$date": "..." } or { "$date": { "$numberLong": "..." } }
                                if (element.TryGetProperty("$date", out var dateProp))
                                {
                                    // dateProp can be string or object
                                    if (dateProp.ValueKind == JsonValueKind.String)
                                    {
                                        var ds = dateProp.GetString();
                                        if (DateTime.TryParse(ds, out var dd))
                                            value = dd;
                                        else
                                            value = ds;
                                    }
                                    else if (dateProp.ValueKind == JsonValueKind.Number)
                                    {
                                        if (dateProp.TryGetInt64(out var ms))
                                        {
                                            // treat as milliseconds since epoch
                                            try
                                            {
                                                var epoch = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                                                value = epoch;
                                            }
                                            catch
                                            {
                                                value = ms;
                                            }
                                        }
                                    }
                                    else if (dateProp.ValueKind == JsonValueKind.Object && dateProp.TryGetProperty("$numberLong", out var numLong))
                                    {
                                        var numStr = numLong.GetString();
                                        if (long.TryParse(numStr, out var numVal))
                                        {
                                            try
                                            {
                                                var epoch = DateTimeOffset.FromUnixTimeMilliseconds(numVal).UtcDateTime;
                                                value = epoch;
                                            }
                                            catch
                                            {
                                                value = numVal;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // fallback to raw
                                        value = element.GetRawText();
                                    }
                                }
                                else
                                {
                                    // Not a $date wrapper - store as BsonDocument if possible
                                    try
                                    {
                                        value = BsonDocument.Parse(element.GetRawText());
                                    }
                                    catch
                                    {
                                        value = element.GetRawText();
                                    }
                                }
                                break;
                            case JsonValueKind.Array:
                                // store arrays as BsonArray
                                try
                                {
                                    var doc = BsonDocument.Parse(element.GetRawText());
                                    value = doc;
                                }
                                catch
                                {
                                    value = element.GetRawText();
                                }
                                break;
                            default:
                                value = element.GetRawText();
                                break;
                        }
                    }

                    if (value == DBNull.Value)
                        value = BsonNull.Value;

                    // Ensure DateTime values become BSON Date types
                    BsonValue bsonVal;
                    if (value is DateTime dtVal)
                    {
                        bsonVal = new BsonDateTime(dtVal);
                    }
                    else if (value is DateTimeOffset dtoVal)
                    {
                        bsonVal = new BsonDateTime(dtoVal.UtcDateTime);
                    }
                    else if (value is BsonValue bv)
                    {
                        bsonVal = bv;
                    }
                    else
                    {
                        bsonVal = BsonValue.Create(value);
                    }

                    updateDefs.Add(updater.Set(kvp.Key, bsonVal));
                }

                if (updateDefs.Count > 0)
                {
                    var filter = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, studentId);
                    var combined = updater.Combine(updateDefs);
                    CommonItems._mongoDBContext.Students.UpdateOne(filter, combined);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating student data: {ex.Message}");
            }
        }
        #endregion
        private void DeleteStudent(int studentId)
        {
            #region OldQuery
            //var parameters = new SqlParameter[]
            //{
            //    new("@studentId", studentId)
            //};
            //string query = "DELETE FROM Student_Data WHERE Student_ID = @studentId";
            //_dbContext.DeleteData(query, parameters);
            #endregion
            #region MongoDB
            var filter = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, studentId);
            CommonItems._mongoDBContext.Students.DeleteOne(filter);
            #endregion
        }

        private void MenuItemReject_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRejectRequest())
                return;

            if (RequestsGrid.SelectedItem is RequestGridModel row)
            {
                var reasonWindow = new RejectReasonWindow();
                if (reasonWindow.ShowDialog() == true)
                {
                    try
                    {
                        #region oldQuery
                        //var parameters = new SqlParameter[]
                        //{
                        //    new("@requestId", Convert.ToInt32(row.Request_ID)),
                        //    new("@rejectedReason", reasonWindow.Reason),
                        //    new("@approvedBy", _currentUserName),
                        //    new("@approvedDate", DateTime.Now)
                        //};
                        //string query = @"
                        //    UPDATE Requests 
                        //    SET Status = 'Rejected',
                        //        RejectedReason = @rejectedReason,
                        //        ApprovedBy = @approvedBy,
                        //        ApprovedDate = @approvedDate
                        //    WHERE Request_ID = @requestId";
                        //_dbContext.UpdateData(query, parameters);
                        #endregion
                        #region MongoDB
                        var filter = Builders<RequestTable>.Filter.Eq(x=>x.Request_ID, Convert.ToInt32(row.Request_ID));
                        var update = Builders<RequestTable>.Update
                                    .Set(x => x.Status, "Rejected")
                                    .Set(x => x.RejectedReason, reasonWindow.Reason)
                                    .Set(x => x.ApprovedBy, _currentUserName)
                                    .Set(x => x.ApprovedDate, DateTime.Now);
                        CommonItems._mongoDBContext.Requests.UpdateOne(filter, update);
                        #endregion
                        LoadRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error rejecting request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanRejectRequest()
        {
            if (RequestsGrid.SelectedItem is not RequestGridModel row)
            {
                MessageBox.Show("Please select a request to reject.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string requestType = row.RequestType.ToString();
            string status = row.Status.ToString();

            if (status != "Open")
            {
                MessageBox.Show("Only open requests can be rejected.", "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_currentUserType == "Master" && requestType != "Update")
            {
                MessageBox.Show("Masters can only reject Update requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}