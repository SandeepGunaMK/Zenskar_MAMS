using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Zenskar_MAMS.Windows
{
    public partial class RequestsWindow : Window
    {
        private readonly DBContext _dbContext;
        private readonly string _currentUserType;
        private readonly string _currentUserName;

        public RequestsWindow(string userType, string userName)
        {
            InitializeComponent();
            _dbContext = new DBContext();
            _currentUserType = userType;
            _currentUserName = userName;

            ConfigurePermissions();
            LoadRequests();
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
            try
            {
                StringBuilder query = new StringBuilder(@"
                    SELECT R.*, 
                           CASE 
                               WHEN R.Student_ID IS NOT NULL THEN S.Name 
                               ELSE NULL 
                           END AS StudentName
                    FROM Requests R
                    LEFT JOIN Student_Data S ON R.Student_ID = S.Student_ID
                    WHERE 1=1");

                var conditions = new List<string>();
                var parameters = new List<SqlParameter>();

                // Status filters
                var statusFilters = new List<string>();
                if (ChkOpen.IsChecked == true) statusFilters.Add("'Open'");
                if (ChkApproved.IsChecked == true) statusFilters.Add("'Approved'");
                if (ChkRejected.IsChecked == true) statusFilters.Add("'Rejected'");

                if (statusFilters.Count > 0)
                {
                    conditions.Add($"R.Status IN ({string.Join(",", statusFilters)})");
                }

                // Type filters
                var typeFilters = new List<string>();
                if (ChkRegistration.IsChecked == true) typeFilters.Add("'Registration'");
                if (ChkUpdate.IsChecked == true) typeFilters.Add("'Update'");
                if (ChkDelete.IsChecked == true) typeFilters.Add("'Delete'");

                if (typeFilters.Count > 0)
                {
                    conditions.Add($"R.RequestType IN ({string.Join(",", typeFilters)})");
                }

                // For non-admin users, show only relevant requests
                if (_currentUserType != "Admin")
                {
                    if (_currentUserType == "Master")
                    {
                        conditions.Add("(R.RequestType = 'Update' OR R.RequestedBy = @userName)");
                        parameters.Add(new SqlParameter("@userName", _currentUserName));
                    }
                    else // Instructor
                    {
                        conditions.Add("R.RequestedBy = @userName");
                        parameters.Add(new SqlParameter("@userName", _currentUserName));
                    }
                }

                if (conditions.Count > 0)
                {
                    query.Append(" AND ").Append(string.Join(" AND ", conditions));
                }

                query.Append(" ORDER BY R.RequestedDate DESC");

                var result = _dbContext.SelectData(query.ToString(), parameters.ToArray());
                RequestsGrid.ItemsSource = result.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (RequestsGrid.SelectedItem is DataRowView row)
            {
                string requestType = row["RequestType"].ToString();
                if (requestType == "Registration")
                {
                    ShowRegistrationDetails(row);
                }
                else if (int.TryParse(row["Student_ID"]?.ToString(), out int studentId))
                {
                    var studentDetails = new StudentDetails(studentId, _currentUserType, _currentUserName);
                    studentDetails.ShowDialog();
                }
            }
        }

        private void ShowRegistrationDetails(DataRowView request)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new("@userName", request["RequestedBy"].ToString())
                };

                string query = "SELECT * FROM User_Table WHERE User_Name = @userName";
                var result = _dbContext.SelectData(query, parameters);

                if (result.Rows.Count > 0)
                {
                    var details = result.Rows[0];
                    string message = $"Registration Details:\n\n" +
                                   $"Name: {details["User_Name"]}\n" +
                                   $"Login ID: {details["login_ID"]}\n" +
                                   $"Contact: {details["Contact_Number"]}\n" +
                                   $"User Type: {details["User_Type"]}\n" +
                                   $"Created Date: {details["Created_Date"]}";

                    MessageBox.Show(message, "Registration Details", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading registration details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemApprove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanApproveRequest())
                return;

            if (RequestsGrid.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Are you sure you want to approve this request?", "Confirm Approval",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ProcessApproval(row);
                        LoadRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error approving request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanApproveRequest()
        {
            if (RequestsGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Please select a request to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string requestType = row["RequestType"].ToString();
            string status = row["Status"].ToString();

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

        private void ProcessApproval(DataRowView row)
        {
            string requestType = row["RequestType"].ToString();
            int requestId = Convert.ToInt32(row["Request_ID"]);

            switch (requestType)
            {
                case "Registration":
                    ApproveRegistration(row["RequestedBy"].ToString());
                    break;
                case "Update":
                    // Student updates are handled directly in StudentDetails
                    break;
                case "Delete":
                    if (int.TryParse(row["Student_ID"]?.ToString(), out int studentId))
                    {
                        DeleteStudent(studentId);
                    }
                    break;
            }

            // Update request status
            var parameters = new SqlParameter[]
            {
                new("@requestId", requestId),
                new("@approvedBy", _currentUserName),
                new("@approvedDate", DateTime.Now)
            };

            string updateQuery = @"
                UPDATE Requests 
                SET Status = 'Approved',
                    ApprovedBy = @approvedBy,
                    ApprovedDate = @approvedDate
                WHERE Request_ID = @requestId";

            _dbContext.UpdateData(updateQuery, parameters);
        }

        private void ApproveRegistration(string userName)
        {
            var parameters = new SqlParameter[]
            {
                new("@userName", userName),
                new("@approvedBy", _currentUserName),
                new("@approvedDate", DateTime.Now)
            };

            string query = @"
                UPDATE User_Table 
                SET Status = 'Active',
                    Approved_By = @approvedBy,
                    Approved_Date = @approvedDate
                WHERE User_Name = @userName";

            _dbContext.UpdateData(query, parameters);
        }

        private void DeleteStudent(int studentId)
        {
            var parameters = new SqlParameter[]
            {
                new("@studentId", studentId)
            };

            string query = "DELETE FROM Student_Data WHERE Student_ID = @studentId";
            _dbContext.DeleteData(query, parameters);
        }

        private void MenuItemReject_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRejectRequest())
                return;

            if (RequestsGrid.SelectedItem is DataRowView row)
            {
                var reasonWindow = new RejectReasonWindow();
                if (reasonWindow.ShowDialog() == true)
                {
                    try
                    {
                        var parameters = new SqlParameter[]
                        {
                            new("@requestId", Convert.ToInt32(row["Request_ID"])),
                            new("@rejectedReason", reasonWindow.Reason),
                            new("@approvedBy", _currentUserName),
                            new("@approvedDate", DateTime.Now)
                        };

                        string query = @"
                            UPDATE Requests 
                            SET Status = 'Rejected',
                                RejectedReason = @rejectedReason,
                                ApprovedBy = @approvedBy,
                                ApprovedDate = @approvedDate
                            WHERE Request_ID = @requestId";

                        _dbContext.UpdateData(query, parameters);
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
            if (RequestsGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Please select a request to reject.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string requestType = row["RequestType"].ToString();
            string status = row["Status"].ToString();

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