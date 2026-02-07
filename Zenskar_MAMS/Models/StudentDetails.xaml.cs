using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class StudentDetails : Window
    {
        private readonly DBContext _dbContext;
        private readonly int _studentId;
        private readonly string _currentUserType;
        private readonly string _currentUserName;
        private bool _isNewStudent;

        public StudentDetails(int studentId, string userType, string userName)
        {
            InitializeComponent();
            _dbContext = new DBContext();
            _studentId = studentId;
            _currentUserType = userType;
            _currentUserName = userName;
            _isNewStudent = studentId == 0;

            LoadInstructorsAndMasters();
            ConfigurePermissions();
            
            if (!_isNewStudent)
            {
                LoadStudentData();
            }
            else
            {
                HeaderText.Text = "Add New Student";
                DpDateOfJoining.SelectedDate = DateTime.Today;
            }

            // Add event handler for DOB change
            DpDOB.SelectedDateChanged += DpDOB_SelectedDateChanged;
        }

        private void LoadInstructorsAndMasters()
        {
            try
            {
                #region OldQuery
                //Load Instructors
                //var instructorsQuery = "SELECT User_Name FROM User_Table WHERE User_Type = 'Instructor' AND Status = 'Active'";
                //var instructors = _dbContext.SelectData(instructorsQuery);
                //foreach (DataRow row in instructors.Rows)
                //{
                //    CmbInstructor.Items.Add(row["User_Name"].ToString());
                //}
                #endregion
                #region MongoDB
                var filter = Builders<UserTable>.Filter.Eq(x => x.User_Type, "Instructor") & Builders<UserTable>.Filter.Eq(x => x.Status, "Active");
                var instructorsCol = CommonItems._mongoDBContext.Users
                    .Find(filter)
                    .Project(x => new { x.User_Name })
                    .ToList();
                foreach (var i in instructorsCol)
                {
                    CmbInstructor.Items.Add(i.User_Name.ToString());
                }
                #endregion

                #region OldQuery
                // Load Masters
                //var mastersQuery = "SELECT User_Name FROM User_Table WHERE User_Type = 'Master' AND Status = 'Active'";
                //var masters = _dbContext.SelectData(mastersQuery);
                //foreach (DataRow row in masters.Rows)
                //{
                //    CmbMaster.Items.Add(row["User_Name"].ToString());
                //}
                #endregion
                #region MongoDB
                var filterMas = Builders<UserTable>.Filter.Eq(x => x.User_Type, "Master") & Builders<UserTable>.Filter.Eq(x => x.Status, "Active");
                var masterCol = CommonItems._mongoDBContext.Users
                    .Find(filterMas)
                    .Project(x => new { x.User_Name })
                    .ToList();
                foreach (var i in masterCol)
                {
                    CmbMaster.Items.Add(i.User_Name.ToString());
                }
                #endregion

                // Set default instructor if current user is an instructor
                if (_currentUserType == "Instructor")
                {
                    CmbInstructor.SelectedItem = _currentUserName;
                    CmbInstructor.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading instructors and masters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurePermissions()
        {
            bool canEdit = false;
            bool canRequestUpdate = false;

            switch (_currentUserType)
            {
                case "Admin":
                    canEdit = true;
                    break;
                case "Master":
                    canEdit = true;
                    break;
                case "Instructor":
                    if (_isNewStudent)
                    {
                        canEdit = true;
                    }
                    else
                    {
                        #region OldQuery
                        // Check if instructor owns this student
                        //var query = "SELECT InstructorName FROM Student_Data WHERE Student_ID = @studentId";
                        //var parameters = new SqlParameter[] { new("@studentId", _studentId) };
                        //var result = _dbContext.SelectData(query, parameters);
                        #endregion
                        #region MongoDB
                        var filterStudent = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, _studentId);
                        var instructors = CommonItems._mongoDBContext.Students
                            .Find(filterStudent)
                            .Project(x => new { x.InstructorName }).ToList();
                        DataTable resultIns = new DataTable();
                        resultIns = CommonItems.ToDataTable(instructors);
                        #endregion


                        if (resultIns.Rows.Count > 0)
                        {
                            if (resultIns.Rows[0]["InstructorName"].ToString() == _currentUserName)
                            {
                                canEdit = true;
                            }
                            else
                            {
                                canRequestUpdate = true;
                                BtnSave.Visibility = Visibility.Collapsed;
                                BtnRequestUpdate.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    break;
            }

            // If the instructor can request an update, enable controls but hide save button
            if (canRequestUpdate)
            {
                return; // Keep all controls enabled for update request
            }

            // Otherwise, apply normal permissions
            if (!canEdit)
            {
                foreach (var element in new FrameworkElement[] 
                { 
                    TxtName, DpDOB, CmbGender, TxtLocation, CmbBelt, CmbInstructor, CmbMaster,
                    TxtContactNumber, TxtParentsName, TxtMedicalConditions, DpLastExamDate,
                    TxtAttempts, DpDateOfJoining, TxtComments 
                })
                {
                    element.IsEnabled = false;
                }
            }
        }

        private void LoadStudentData()
        {
            try
            {
                #region OldQuery
                //var parameters = new SqlParameter[] { new("@studentId", _studentId) };
                //string query = "SELECT * FROM Student_Data WHERE Student_ID = @studentId";
                //var result = _dbContext.SelectData(query, parameters);
                #endregion
                #region MongoDB
                var projectionAll = Builders<StudentTable>.Projection.Exclude("_id");
                var filter = Builders<StudentTable>.Filter.Eq(x => x.Student_ID, _studentId);
                var studentData = CommonItems._mongoDBContext.Students
                    .Find(filter)
                    .Project<StudentTable>(projectionAll).ToList();
                DataTable resultstudents = new DataTable();
                resultstudents = CommonItems.ToDataTable(studentData);
                #endregion

                if (resultstudents.Rows.Count > 0)
                {
                    var student = resultstudents.Rows[0];
                    TxtName.Text = student["Name"].ToString();
                    DpDOB.SelectedDate = Convert.ToDateTime(student["DOB"]);
                    TxtAge.Text = student["Age"].ToString();
                    CmbGender.Text = student["Gender"].ToString();
                    TxtLocation.Text = student["Location"].ToString();
                    CmbBelt.Text = student["Belt"].ToString();
                    CmbInstructor.Text = student["InstructorName"].ToString();
                    CmbMaster.Text = student["MasterName"].ToString();
                    TxtContactNumber.Text = student["ContactNumber"].ToString();
                    TxtParentsName.Text = student["ParentsName"].ToString();
                    TxtMedicalConditions.Text = student["MedicalConditions"].ToString();
                    
                    if (student["LastExamDate"] != DBNull.Value)
                        DpLastExamDate.SelectedDate = Convert.ToDateTime(student["LastExamDate"]);
                    
                    TxtAttempts.Text = student["Attempts"].ToString();
                    DpDateOfJoining.SelectedDate = Convert.ToDateTime(student["DateOfJoining"]);
                    TxtComments.Text = student["Comments"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DpDOB_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpDOB.SelectedDate.HasValue)
            {
                var age = DateTime.Today.Year - DpDOB.SelectedDate.Value.Year;
                if (DpDOB.SelectedDate.Value.Date > DateTime.Today.AddYears(-age))
                    age--;
                TxtAge.Text = age.ToString();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Please enter the student's name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DpDOB.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select the date of birth.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbGender.SelectedItem == null)
            {
                MessageBox.Show("Please select the gender.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLocation.Text))
            {
                MessageBox.Show("Please enter the location.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbBelt.SelectedItem == null)
            {
                MessageBox.Show("Please select the belt.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbInstructor.SelectedItem == null)
            {
                MessageBox.Show("Please select an instructor.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Please select a master.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtContactNumber.Text))
            {
                MessageBox.Show("Please enter the contact number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtParentsName.Text))
            {
                MessageBox.Show("Please enter the parents' name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DpDateOfJoining.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select the date of joining.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var parameters = new SqlParameter[]
                {
                    new("@name", TxtName.Text),
                    new("@dob", DpDOB.SelectedDate.Value),
                    new("@age", int.Parse(TxtAge.Text)),
                    new("@gender", CmbGender.Text),
                    new("@location", TxtLocation.Text),
                    new("@belt", CmbBelt.Text),
                    new("@instructorName", CmbInstructor.Text),
                    new("@masterName", CmbMaster.Text),
                    new("@contactNumber", TxtContactNumber.Text),
                    new("@parentsName", TxtParentsName.Text),
                    new("@medicalConditions", TxtMedicalConditions.Text ?? string.Empty),
                    new("@lastExamDate", (object)DpLastExamDate.SelectedDate ?? DBNull.Value),
                    new("@attempts", string.IsNullOrEmpty(TxtAttempts.Text) ? 0 : int.Parse(TxtAttempts.Text)),
                    new("@dateOfJoining", DpDateOfJoining.SelectedDate.Value),
                    new("@comments", TxtComments.Text ?? string.Empty)
                };
                #region OldQuery
                //if (_isNewStudent)
                //{
                //string insertQuery = @"
                //    INSERT INTO Student_Data (
                //        Name, DOB, Age, Gender, Location, Belt, InstructorName, MasterName,
                //        ContactNumber, ParentsName, MedicalConditions, LastExamDate, Attempts,
                //        DateOfJoining, Comments, StudentStatus
                //    )
                //    VALUES (
                //        @name, @dob, @age, @gender, @location, @belt, @instructorName, @masterName,
                //        @contactNumber, @parentsName, @medicalConditions, @lastExamDate, @attempts,
                //        @dateOfJoining, @comments, 'Active'
                //    )";

                //_dbContext.InsertData(insertQuery, parameters);
                #endregion
                #region MongoDB
                if (_isNewStudent)
                {
                    var studentDoc = new StudentTable
                    {
                        Name = TxtName.Text,
                        DOB = DpDOB.SelectedDate.Value,
                        Age = int.Parse(TxtAge.Text),
                        Gender = CmbGender.Text,
                        Location = TxtLocation.Text,
                        Belt = CmbBelt.Text,
                        InstructorName = CmbInstructor.Text,
                        MasterName = CmbMaster.Text,
                        ContactNumber = TxtContactNumber.Text,
                        ParentsName = TxtParentsName.Text,
                        MedicalConditions = TxtMedicalConditions.Text ?? string.Empty,
                        LastExamDate = DpLastExamDate.SelectedDate,
                        Attempts = string.IsNullOrEmpty(TxtAttempts.Text) ? 0 : int.Parse(TxtAttempts.Text),
                        DateOfJoining = DpDateOfJoining.SelectedDate.Value,
                        Comments = TxtComments.Text ?? string.Empty,
                        StudentStatus = "Active"
                    };

                    CommonItems._mongoDBContext.Students.InsertOne(studentDoc);
                    #endregion

                    MessageBox.Show("Student added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    #region OldQuery
                    //var SelectParameters = new SqlParameter[] {new("@name", TxtName.Text), new("@parentsName", TxtParentsName.Text) };                                
                    //string SelectStudentId = @"Select Student_ID From Student_Data where Name = @name and ParentsName = @parentsName";
                    //var StuDentId = _dbContext.SelectData(SelectStudentId, SelectParameters);
                    #endregion
                    #region MongoDB
                    var filterStudent = Builders<StudentTable>.Filter.Eq(x => x.ParentsName, TxtParentsName.Text);
                    var StuDentId = CommonItems._mongoDBContext.Students
                        .Find(filterStudent)
                        .Project(x => new { x.Student_ID }).ToList();
                    DataTable resultStuDentId = new DataTable();
                    resultStuDentId = CommonItems.ToDataTable(StuDentId);
                    int studentId = Convert.ToInt32(resultStuDentId.Rows[0]["Student_ID"]);
                    #endregion

                    #region OldQuery
                    //   string insertQuery2 = @"
                    //       INSERT INTO Attendance ( Student_ID, Name, January, February, March, April, May, June, July, August, September, October, November, December )
                    //       VALUES (@StudentId, @name, '00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025','00-00-2025')";

                    //   var parameters2 = new SqlParameter[]
                    //{
                    //   new("@name", TxtName.Text),new("@StudentId", StuDentId.Rows[0]["Student_ID"])
                    //};
                    //   _dbContext.InsertData(insertQuery2, parameters2);
                    #endregion
                    #region MongoDB
                    var attendanceDoc = new AttendanceTable
                    {
                        Student_ID = studentId,
                        Name = TxtName.Text,
                        January = "00-00-2025",
                        February = "00-00-2025",
                        March = "00-00-2025",
                        April = "00-00-2025",
                        May = "00-00-2025",
                        June = "00-00-2025",
                        July = "00-00-2025",
                        August = "00-00-2025",
                        September = "00-00-2025",
                        October = "00-00-2025",
                        November = "00-00-2025",
                        December = "00-00-2025"
                    };

                    CommonItems._mongoDBContext.Attendance.InsertOne(attendanceDoc);
                    #endregion
                    MessageBox.Show("Student's Attendance added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);


                }
                else
                {
                    var parametersList = parameters.ToList();
                    parametersList.Add(new SqlParameter("@studentId", _studentId));

                    string updateQuery = @"
                        UPDATE Student_Data SET
                            Name = @name,
                            DOB = @dob,
                            Age = @age,
                            Gender = @gender,
                            Location = @location,
                            Belt = @belt,
                            InstructorName = @instructorName,
                            MasterName = @masterName,
                            ContactNumber = @contactNumber,
                            ParentsName = @parentsName,
                            MedicalConditions = @medicalConditions,
                            LastExamDate = @lastExamDate,
                            Attempts = @attempts,
                            DateOfJoining = @dateOfJoining,
                            Comments = @comments
                        WHERE Student_ID = @studentId";

                    _dbContext.UpdateData(updateQuery, parametersList.ToArray());
                    MessageBox.Show("Student updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving student data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRequestUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // First, serialize the updated data
                var updatedData = new
                {
                    Name = TxtName.Text,
                    DOB = DpDOB.SelectedDate.Value,
                    Age = int.Parse(TxtAge.Text),
                    Gender = CmbGender.Text,
                    Location = TxtLocation.Text,
                    Belt = CmbBelt.Text,
                    InstructorName = CmbInstructor.Text,
                    MasterName = CmbMaster.Text,
                    ContactNumber = TxtContactNumber.Text,
                    ParentsName = TxtParentsName.Text,
                    MedicalConditions = TxtMedicalConditions.Text ?? string.Empty,
                    LastExamDate = (object)DpLastExamDate.SelectedDate ?? DBNull.Value,
                    Attempts = string.IsNullOrEmpty(TxtAttempts.Text) ? 0 : int.Parse(TxtAttempts.Text),
                    DateOfJoining = DpDateOfJoining.SelectedDate.Value,
                    Comments = TxtComments.Text ?? string.Empty
                };

                // Convert the updated data to JSON
                string updatedDataJson = System.Text.Json.JsonSerializer.Serialize(updatedData);

                var parameters = new SqlParameter[]
                {
                    new("@requestType", "Update"),
                    new("@requestedBy", _currentUserName),
                    new("@studentId", _studentId),
                    new("@requestedDate", DateTime.Now),
                    new("@updatedData", updatedDataJson)
                };

                string query = @"
                    INSERT INTO Requests (
                        RequestType, 
                        RequestedBy, 
                        Student_ID, 
                        Status, 
                        RequestedDate, 
                        UpdatedData
                    )
                    VALUES (
                        @requestType, 
                        @requestedBy, 
                        @studentId, 
                        'Open', 
                        @requestedDate,
                        @updatedData
                    )";

                _dbContext.InsertData(query, parameters);
                MessageBox.Show("Update request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting update request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}