using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public class BatchItem
    {
        public string Location { get; set; } 
        public string Batch { get; set; }
    }
    public partial class StudentsList : Window
    {
        //private readonly DBContext _dbContext;
        private readonly string _userType;
        private readonly string _userName;
        private DataTable _originalData;
        private DataTable _filteredData;
        private Dictionary<string, HashSet<string>> _columnFilters;
        private ICollectionView _studentsView;

        #region Dropdown Filter Properties
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<BatchItem> _monthYear_BF;
        public ObservableCollection<BatchItem> MonthYear_BF
        {
            get => _monthYear_BF;
            set { _monthYear_BF = value; OnPropertyChanged(nameof(MonthYear_BF)); }
        }

        private string _selectedMonthYear = "All";
        public string SelectedMonthYear
        {
            get => _selectedMonthYear;
            set { _selectedMonthYear = value; OnPropertyChanged(nameof(SelectedMonthYear)); }
        }

        private ObservableCollection<BatchItem> _lastExam_BF;
        public ObservableCollection<BatchItem> LastExam_BF
        {
            get => _lastExam_BF;
            set { _lastExam_BF = value; OnPropertyChanged(nameof(LastExam_BF)); }
        }

        private string _selectedLastExam = "All";
        public string SelectedLastExam
        {
            get => _selectedLastExam;
            set { _selectedLastExam = value; OnPropertyChanged(nameof(SelectedLastExam)); }
        }

        private ObservableCollection<BatchItem> _location_BF;
        public ObservableCollection<BatchItem> Location_BF
        {
            get => _location_BF;
            set { _location_BF = value; OnPropertyChanged(nameof(Location_BF)); }
        }

        private string _selectedLocation = "All";
        public string SelectedLocation
        {
            get => _selectedLocation;
            set { _selectedLocation = value; OnPropertyChanged(nameof(SelectedLocation)); }
        }

        private ObservableCollection<BatchItem> _batch_BF;
        public ObservableCollection<BatchItem> Batch_BF
        {
            get => _batch_BF;
            set { _batch_BF = value; OnPropertyChanged(nameof(Batch_BF)); }
        }

        private string _selectedBatch = "All";
        public string SelectedBatch 
        {
            get => _selectedBatch;
            set { _selectedBatch = value; OnPropertyChanged(nameof(SelectedBatch)); }
        }

        private ObservableCollection<BatchItem> _status_BF;
        public ObservableCollection<BatchItem> Status_BF
        {
            get => _status_BF;
            set { _status_BF = value; OnPropertyChanged(nameof(Status_BF)); }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        private ObservableCollection<BatchItem> _instructor_BF;
        public ObservableCollection<BatchItem> Instructor_BF
        {
            get => _instructor_BF;
            set { _instructor_BF = value; OnPropertyChanged(nameof(Instructor_BF)); }
        }

        private string _selectedInstructor = "All";
        public string SelectedInstructor
        {
            get => _selectedInstructor;
            set { _selectedInstructor = value; OnPropertyChanged(nameof(SelectedInstructor)); }
        }

        private ObservableCollection<BatchItem> _master_BF;
        public ObservableCollection<BatchItem> Master_BF
        {
            get => _master_BF;
            set { _master_BF = value; OnPropertyChanged(nameof(Master_BF)); }
        }

        private string _selectedMaster = "All";
        public string SelectedMaster
        {
            get => _selectedMaster;
            set { _selectedMaster = value; OnPropertyChanged(nameof(SelectedMaster)); }
        }

        private ObservableCollection<BatchItem> _gender_BF;
        public ObservableCollection<BatchItem> Gender_BF
        {
            get => _gender_BF;
            set { _gender_BF = value; OnPropertyChanged(nameof(Gender_BF)); }
        }

        private string _selectedGender = "All";
        public string SelectedGender
        {
            get => _selectedGender;
            set { _selectedGender = value; OnPropertyChanged(nameof(SelectedGender)); }
        }

        #endregion

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 1)
            {
                Application.Current.Shutdown();
            }
        }
        public StudentsList(string userType, string userName)
        {
            InitializeComponent();
            DataContext = this;
            //_dbContext = new DBContext();
            _userType = userType;
            _userName = userName;
            _columnFilters = new Dictionary<string, HashSet<string>>();
            if(userType == "Instructor") { UpdateInstructor.Visibility = Visibility.Hidden; }            
            LoadStudents();
            
            ConfigureUserPermissions();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutHelper.Logout(this); OnClosed(e);
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(sender, e, _filteredData);
        }


        private void ExportToExcel(object sender, RoutedEventArgs e, DataTable studentTable)
        {
            try
            {
                // Ask user for location
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = "Zenskar_List.xlsx"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Students_List");

                    // Write headers
                    ws.Cell(1, 1).Value = "SlNo"; ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 2).Value = "Name"; ws.Cell(1, 2).Style.Font.Bold = true;
                    ws.Cell(1, 3).Value = "Age"; ws.Cell(1, 3).Style.Font.Bold = true;
                    ws.Cell(1, 4).Value = "Location"; ws.Cell(1, 4).Style.Font.Bold = true;
                    ws.Cell(1, 5).Value = "Instructor Name"; ws.Cell(1, 5).Style.Font.Bold = true;
                    ws.Cell(1, 6).Value = "Master Name"; ws.Cell(1, 6).Style.Font.Bold = true;
                    ws.Cell(1, 7).Value = "Remarks"; ws.Cell(1, 7).Style.Font.Bold = true;

                    int currentRow = 2;
                    int slNo = 1;
                    // Split data into groups
                    var boysKids = studentTable.AsEnumerable()
                        .Where(r => r["Gender"].ToString().Equals("Male", StringComparison.OrdinalIgnoreCase)
                                 && Convert.ToInt32(r["Age"]) <= 15);
                    var girlsKids = studentTable.AsEnumerable()
                        .Where(r => r["Gender"].ToString().Equals("Female", StringComparison.OrdinalIgnoreCase)
                                 && Convert.ToInt32(r["Age"]) <= 15);
                    var boysAdults = studentTable.AsEnumerable()
                        .Where(r => r["Gender"].ToString().Equals("Male", StringComparison.OrdinalIgnoreCase)
                                 && Convert.ToInt32(r["Age"]) > 15);
                    var girlsAdults = studentTable.AsEnumerable()
                        .Where(r => r["Gender"].ToString().Equals("Female", StringComparison.OrdinalIgnoreCase)
                                 && Convert.ToInt32(r["Age"]) > 15);

                    // Helper function to write each group
                    void WriteGroup(string groupName, IEnumerable<DataRow> rows)
                    {
                        ws.Cell(currentRow, 1).Value = groupName;
                        ws.Range(currentRow, 1, currentRow, 7).Merge();
                        ws.Row(currentRow).Style.Font.Bold = true;
                        currentRow++;

                        
                        foreach (var row in rows)
                        {
                            ws.Cell(currentRow, 1).Value = slNo++;
                            ws.Cell(currentRow, 2).Value = row["Name"]?.ToString() ?? "";
                            ws.Cell(currentRow, 3).Value = Convert.ToInt32(row["Age"]);
                            ws.Cell(currentRow, 4).Value = row["Location"]?.ToString() ?? "";
                            ws.Cell(currentRow, 5).Value = row["InstructorName"]?.ToString() ?? "";
                            ws.Cell(currentRow, 6).Value = row["MasterName"]?.ToString() ?? "";
                            ws.Cell(currentRow, 7).Value = ""; // Remarks column empty
                            currentRow++;
                        }

                        //currentRow++; // Blank line between groups
                    }

                    // Write groups
                    WriteGroup("Girls Kids", girlsKids);
                    WriteGroup("Boys Kids", boysKids);
                    WriteGroup("Girls Adults", girlsAdults);
                    WriteGroup("Boys Adults", boysAdults);

                    // Apply styling
                    ws.Columns().AdjustToContents();
                    ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    wb.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show("Excel Export Completed Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during export:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadStudents()
        {
            try
            {
                #region OldQuery
                //string condition = "CASE \r\n" +
                //    "WHEN Belt = 'Non-Uniform' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 45 THEN 'Yes'\r\n" +
                //    "WHEN Belt = 'White' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 45 THEN 'Yes'\r\n" +
                //    "WHEN Belt = 'White Senior' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 60 THEN 'Yes'\r\n" +
                //    "WHEN Belt = 'Yellow' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 90 THEN 'Yes'\r\n" +
                //    "ELSE 'No'" +
                //    "END AS 'ExamDue' ";

                //string query = "SELECT *," + condition + " FROM Student_Data";
                //_originalData = _dbContext.SelectData(query);
                #endregion
                #region MongoDBQuery
                var projectionAll = Builders<StudentTable>.Projection.Exclude("_id");
                var filter = FilterDefinition<StudentTable>.Empty;
                var studentData = CommonItems._mongoDBContext.Students
                    .Find(filter)
                    .Project<StudentTable>(projectionAll).ToList();
                _originalData = CommonItems.ToDataTable(studentData);
                _originalData.Columns.Add("ExamDue", typeof(string));

                foreach (DataRow row in _originalData.Rows)
                {
                    if (row["LastExamDate"] == DBNull.Value || row["Belt"] == DBNull.Value)
                    {
                        row["ExamDue"] = "No";
                        continue;
                    }
                    string belt = row["Belt"].ToString();
                    DateTime lastExamDate = Convert.ToDateTime(row["LastExamDate"]);
                    int days = (DateTime.Now - lastExamDate).Days;
                    bool examDue =
                                    (belt == "Non-Uniform" && days > 45) ||
                                    (belt == "White" && days > 45) ||
                                    (belt == "White Senior" && days > 60) ||
                                    (belt == "Yellow" && days > 90);

                    row["ExamDue"] = examDue ? "Yes" : "No";
                }
                #endregion
                //var parameters = _userType == "Instructor" 
                //    ? new SqlParameter[] { new("@userName", _userName) }
                //    : Array.Empty<SqlParameter>();



                StudentsGrid.ItemsSource = _originalData.DefaultView;

                // Set alternating row colors
                StudentsGrid.AlternationCount = 2;
                GetDropDownValues();

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        private void GetDropDownValues()
        {
            try
            {
                #region Populate filter options for DateOfJoining 
                var distinctMonthYears = _originalData.AsEnumerable()
                    .Select(r => r.Field<DateTime>("DateOfJoining"))
                    .Where(v => v != null)
                    .Select(v => v.ToString("MMMM yyyy"))
                    .Distinct()
                    .OrderByDescending(v => DateTime.ParseExact(v, "MMMM yyyy", null))
                    .Select(v => new BatchItem { Batch = v })
                    .ToList();
                distinctMonthYears.Insert(0, new BatchItem { Batch = "All" });
                MonthYear_BF = new ObservableCollection<BatchItem>(distinctMonthYears);
                #endregion

                #region Populate filter options for LastExamDate 
                var distinctLastExam = _originalData.AsEnumerable()
                    .Select(r => r.Field<DateTime>("LastExamDate"))
                    .Where(v => v != null)
                    .Select(v => v.ToString("MMMM yyyy"))
                    .Distinct()
                    .OrderByDescending(v => DateTime.ParseExact(v, "MMMM yyyy", null))
                    .Select(v => new BatchItem { Batch = v })
                    .ToList();
                distinctLastExam.Insert(0, new BatchItem { Batch = "All" });
                LastExam_BF = new ObservableCollection<BatchItem>(distinctLastExam);
                #endregion

                #region Populate filter options for Location 
                var distinctLocations = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("Location"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Location = v }).ToList();
                distinctLocations.Insert(0, new BatchItem { Location = "All" });
                Location_BF = new ObservableCollection<BatchItem>(distinctLocations);
                #endregion
                #region Populate filter options for Batch
                var distinctBatchs = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("Belt"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Batch = v }).ToList();
                distinctBatchs.Insert(0, new BatchItem { Batch = "All" });
                Batch_BF = new ObservableCollection<BatchItem>(distinctBatchs);
                #endregion
                #region Populate filter options for Status
                var distinctStaus = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("StudentStatus"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Batch = v }).ToList();
                distinctStaus.Insert(0, new BatchItem { Batch = "All" });
                Status_BF = new ObservableCollection<BatchItem>(distinctStaus);
                #endregion
                #region Populate filter options for Instructor
                var distinctInstructor = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("InstructorName"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Batch = v }).ToList();
                distinctInstructor.Insert(0, new BatchItem { Batch = "All" });
                Instructor_BF = new ObservableCollection<BatchItem>(distinctInstructor);
                #endregion
                #region Populate filter options for Master
                var distinctMaster = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("MasterName"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Batch = v }).ToList();
                distinctMaster.Insert(0, new BatchItem { Batch = "All" });
                Master_BF = new ObservableCollection<BatchItem>(distinctMaster);
                #endregion
                #region Populate filter options for Gender
                var distinctGender = _originalData.AsEnumerable()
                        .Select(r => r.Field<string>("Gender"))
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Select(v => new BatchItem { Batch = v }).ToList();
                distinctGender.Insert(0, new BatchItem { Batch = "All" });
                Gender_BF = new ObservableCollection<BatchItem>(distinctGender);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Dropdown's Data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BatchFilterSelected(object sender, RoutedEventArgs e)
        {
            DataTable dt = _originalData;
            DataRow[] filteredRows = dt.Select();
            string filter = addfilter();           
            filteredRows = dt.Select(filter);
            if (filteredRows.Length == 0)
            {
                StudentsGrid.ItemsSource = dt.Select("0=1");
                return;
            }
            StudentsGrid.ItemsSource = filteredRows.CopyToDataTable().DefaultView;
            _filteredData = filteredRows.CopyToDataTable();
        }

        private string addfilter()
        {
            string filter = "1=1"; // always true, helps build conditions easily

            //if (SelectedMonthYear != "All")
            //{
            //    DateTime selectedDate = DateTime.ParseExact(SelectedMonthYear, "MMMM yyyy", null);
            //    string startDate = selectedDate.ToString("yyyy-MM-01");
            //    string endDate = selectedDate.AddMonths(1).ToString("yyyy-MM-01");
            //    filter += $" AND DateOfJoining >= '{startDate}' AND DateOfJoining < '{endDate}'";
            //}
            if(!string.IsNullOrEmpty(BatchIDValue.Text))
            {
                filter += $" AND Batch_ID like '%{BatchIDValue.Text}%'";
            }
            if (SelectedLastExam != "All")
            {
                DateTime selectedDate = DateTime.ParseExact(SelectedLastExam, "MMMM yyyy", null);
                string startDate = selectedDate.ToString("yyyy-MM-01");
                string endDate = selectedDate.AddMonths(1).ToString("yyyy-MM-01");
                filter += $" AND LastExamDate >= '{startDate}' AND LastExamDate < '{endDate}'";
            }

            if (SelectedLocation != "All")
                filter += $" AND Location = '{SelectedLocation}'";

            if (SelectedBatch != "All")
                filter += $" AND Belt = '{SelectedBatch}'";

            if (SelectedStatus != "All")
                filter += $" AND StudentStatus = '{SelectedStatus}'";

            if (SelectedInstructor != "All")
                filter += $" AND InstructorName = '{SelectedInstructor}'";

            if (SelectedMaster != "All")
                filter += $" AND MasterName = '{SelectedMaster}'";

            if (SelectedGender != "All")
                filter += $" AND Gender = '{SelectedGender}'";

            string? selectedAge = (Age.SelectedItem as ComboBoxItem)?.Content?.ToString();
            //if (selectedAge != "All")
            //    filter += $" AND Age " + selectedAge + "'" + AgeValue.Text + "'";
            if (string.IsNullOrEmpty(selectedAge) || selectedAge == "All") { filter += $" AND 1=1"; }
            else if (string.IsNullOrEmpty(AgeValue.Text)) { filter += $" AND 1=1"; }
            else { filter += $" AND Age " + selectedAge + "'" + AgeValue.Text + "'"; }

            return filter;
        }
        private void ConfigureUserPermissions()
        {
            switch (_userType)
            {
                case "Admin":
                    BtnDeleteStudent.Visibility = Visibility.Visible;
                    break;
                case "Master":
                    break;
                case "Instructor":
                    break;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (_userType)
            {
                case "Admin":
                    AdminHome adminHome = new AdminHome(_userName);
                    this.Close();
                    adminHome.Show();
                    break;
                case "Master":
                    MasterHome masterHome = new MasterHome(_userName);   
                    this.Close();
                    masterHome.Show();
                    break;
                case "Instructor":
                    InstructorHome instructorHome = new InstructorHome(_userName);
                    this.Close();
                    instructorHome.Show();
                    break;
            }
        }

        private void StudentsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                var studentDetails = new StudentDetails(
                    Convert.ToInt32(row["Student_ID"]),
                    _userType,
                    _userName
                );
                studentDetails.ShowDialog();
                LoadStudents(); // Refresh after details window is closed
            }
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            var studentDetails = new StudentDetails(0, _userType, _userName); // 0 indicates new student
            if (studentDetails.ShowDialog() == true)
            {
                LoadStudents();
            }
        }
        private void BtnViewAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                var studentDetails = new Attendance(
                    Convert.ToInt32(row["Student_ID"]),
                    _userType,
                    _userName
                );
                studentDetails.ShowDialog();
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Please select a student to view Attendance.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                var studentDetails = new StudentDetails(
                    Convert.ToInt32(row["Student_ID"]),
                    _userType,
                    _userName
                );
                studentDetails.ShowDialog();
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Please select a student to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Are you sure you want to Update the status of this student?", "Update", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        #region OldQuery
                        //var parameterSelect = new SqlParameter[]
                        //{
                        //    new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        //};
                        //var parameters = new SqlParameter[]
                        //{
                        //    new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        //};

                        //string query;
                        //string query = "SELECT StudentStatus FROM Student_Data WHERE Student_ID = @studentId";
                        //var result = _dbContext.SelectData(query, parameterSelect);
                        //string status = result.Rows[0]["StudentStatus"].ToString();
                        #endregion
                        #region MongoDBQuery
                        var _studentId = Convert.ToInt32(row["Student_ID"]);
                        var filter = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, _studentId);
                        var statusCol = CommonItems._mongoDBContext.Students
                                                .Find(filter)
                                                .Project(x => new { x.StudentStatus })
                                                .ToList();
                        DataTable dtStatus = CommonItems.ToDataTable(statusCol);
                        string? status = dtStatus.Rows[0]["StudentStatus"].ToString();
                        #endregion
                        if (status != null)
                        {
                            if (status == "Active")
                            {
                                #region OldQuery
                                //query = "UPDATE Student_Data SET StudentStatus = 'Stopped' WHERE Student_ID = @studentId";
                                //_dbContext.UpdateData(query, parameters);
                                #endregion
                                #region MongoDBQuery
                                var filtersts = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, _studentId);
                                var updatests = Builders<StudentTable>.Update.Set(x => x.StudentStatus, "Stopped");
                                CommonItems._mongoDBContext.Students.UpdateOne(filtersts, updatests);
                                #endregion
                                MessageBox.Show("Student status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else if (status == "Stopped")
                            {
                                #region OldQuery
                                //query = "UPDATE Student_Data SET StudentStatus = 'Active' WHERE Student_ID = @studentId";
                                //_dbContext.UpdateData(query, parameters);
                                #endregion
                                #region MongoDBQuery
                                var filtersts = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, _studentId);
                                var updatests = Builders<StudentTable>.Update.Set(x => x.StudentStatus, "Active");
                                CommonItems._mongoDBContext.Students.UpdateOne(filtersts, updatests);
                                #endregion
                                MessageBox.Show("Student status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Student status is neither Active nor Stopped. Please Contact Administrator.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else { MessageBox.Show($"Status is Null, Please Contact Administrator", "Error updating student status", MessageBoxButton.OK, MessageBoxImage.Error); }
                            LoadStudents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error updating student status", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a student to stop.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_userType != "Admin")
            {
                MessageBox.Show("Only administrators can delete students.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Are you sure you want to delete this student? This action cannot be undone.", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        #region OldQuery
                        //var parameters = new SqlParameter[]
                        //{
                        //    new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        //};
                        //string query = "DELETE FROM Student_Data WHERE Student_ID = @studentId";
                        //_dbContext.DeleteData(query, parameters);
                        #endregion
                        #region MongoDBQuery
                        var studentId = Convert.ToInt32(row["Student_ID"]);
                        var filter = Builders<StudentTable>.Filter.Eq("Student_ID", studentId);
                        CommonItems._mongoDBContext.Students.DeleteOne(filter);
                        #endregion
                        LoadStudents();
                        MessageBox.Show("Student Removed from Inventory", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        #region MongoDBQuery
                        var filterAttend = Builders<AttendanceTable>.Filter.Eq("Student_ID", studentId);
                        CommonItems._mongoDBContext.Attendance.DeleteOne(filterAttend);
                        #endregion
                        MessageBox.Show("Student Attendance Removed from Inventory", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a student to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var columnName = button.Tag.ToString();
                columnName = (columnName=="Master") ? "MasterName" : columnName;
                columnName = (columnName=="Status") ? "StudentStatus" : columnName;
                columnName = (columnName=="Instructor") ? "InstructorName" : columnName;
                var dataView = (DataView)StudentsGrid.ItemsSource;
                var data = new DataView(_originalData);

                var filterWindow = new FilterWindow(columnName, data, ApplyColumnFilter);
                filterWindow.Owner = this;
                filterWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing filter: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        
        private void ClearFilters_BtnClick(object sender, RoutedEventArgs e) {
            Location.SelectedValue= "All";
            Batch.SelectedValue= "All";
            Status.SelectedValue= "All";
            Instructor.SelectedValue= "All";
            Master.SelectedValue= "All";
            Gender.SelectedValue= "All";
            Age.SelectedIndex= 0;
            BatchIDValue.Text = null;  
            LastExam.SelectedValue = "All";
            AgeValue.Text= null;
            BatchFilterSelected(sender, e);
        }
        private void UpdateInstructor_BtnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new UpdateInstructor();
            dlg.Owner = this;
            dlg.ShowDialog();
            LoadStudents();
        }

        private void ApplyColumnFilter(string columnName, IEnumerable<string> selectedValues)
        {
            try
            {   
                var view = (DataView)StudentsGrid.ItemsSource;
                view = new DataView(_originalData);
                if (selectedValues == null)
                {
                    // Clear filter
                    view.RowFilter = string.Empty;
                    return;
                }

                if (!selectedValues.Any())
                {
                    return; // No values selected, keep current filter
                }

                var filter = string.Join(" OR ", 
                    selectedValues.Select(v => $"Convert({columnName}, 'System.String') = '{v}'"));

                if (string.IsNullOrEmpty(view.RowFilter))
                    view.RowFilter = $"({filter})";
                else
                    view.RowFilter += $" AND ({filter})";

                StudentsGrid.ItemsSource = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        

        private void StudentsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Prevent automatic sorting
            e.Handled = true;

            var column = e.Column;
            var sortDirection = column.SortDirection == ListSortDirection.Ascending 
                ? ListSortDirection.Descending 
                : ListSortDirection.Ascending;

            column.SortDirection = sortDirection;

            var direction = sortDirection == ListSortDirection.Ascending ? "ASC" : "DESC";
            var header = column.Header as string;

            // You can extend this to sort by multiple columns if needed
            var sortedData = _originalData.AsEnumerable()
                .OrderBy(row => row.Field<object>(header))
                .CopyToDataTable();

            if (sortDirection == ListSortDirection.Descending)
            {
                sortedData = sortedData.AsEnumerable()
                    .Reverse()
                    .CopyToDataTable();
            }

            StudentsGrid.ItemsSource = sortedData.DefaultView;
        }
                
    }

    public class FilterItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        
        public string Value { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}