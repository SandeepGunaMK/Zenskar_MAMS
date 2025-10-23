using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Zenskar_MAMS.Windows
{
    public partial class StudentsList : Window
    {
        private readonly DBContext _dbContext;
        private readonly string _currentUserType;
        private readonly string _currentUserName;

        public StudentsList(string userType, string userName)
        {
            InitializeComponent();
            _dbContext = new DBContext();
            _currentUserType = userType;
            _currentUserName = userName;

            // Configure permissions based on user type
            ConfigureUserPermissions();

            // Load initial data
            LoadStudents();

            // Set default values for filters
            CmbAgeOperator.SelectedIndex = 0;
            CmbGenderFilter.SelectedIndex = 0;
            CmbBeltFilter.SelectedIndex = 0;
            CmbStatusFilter.SelectedIndex = 0;
        }

        private void ConfigureUserPermissions()
        {
            switch (_currentUserType)
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

        private void LoadStudents()
        {
            try
            {
                StringBuilder query = new StringBuilder("SELECT * FROM Student_Data WHERE 1=1");
                //SqlParameter[] param = new SqlParameter[] { new("@instructorName", _currentUserName) };
                //// For instructors, only show their students
                //if (_currentUserType == "Instructor")
                //{                    
                //    query.Append(" AND InstructorName = @instructorName");
                //}
                //var result = _dbContext.SelectData(query.ToString(), param);
                var result = _dbContext.SelectData(query.ToString());
                StudentsGrid.ItemsSource = result.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder query = new StringBuilder("SELECT * FROM Student_Data WHERE 1=1");
                var parameters = new List<SqlParameter>();
                var conditions = new List<string>();

                // Name filter
                if (!string.IsNullOrWhiteSpace(TxtNameFilter.Text))
                {
                    conditions.Add("Name LIKE @name");
                    parameters.Add(new SqlParameter("@name", $"%{TxtNameFilter.Text}%"));
                }

                // Age filter
                if (!string.IsNullOrWhiteSpace(TxtAgeFilter.Text) && int.TryParse(TxtAgeFilter.Text, out int age))
                {
                    string ageOperator = (CmbAgeOperator.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "=";
                    conditions.Add($"Age {ageOperator} @age");
                    parameters.Add(new SqlParameter("@age", age));
                }

                // Gender filter
                if (CmbGenderFilter.SelectedIndex > 0)
                {
                    conditions.Add("Gender = @gender");
                    parameters.Add(new SqlParameter("@gender", (CmbGenderFilter.SelectedItem as ComboBoxItem)?.Content.ToString()));
                }

                // Location filter
                if (!string.IsNullOrWhiteSpace(TxtLocationFilter.Text))
                {
                    conditions.Add("Location LIKE @location");
                    parameters.Add(new SqlParameter("@location", $"%{TxtLocationFilter.Text}%"));
                }

                // Belt filter
                if (CmbBeltFilter.SelectedIndex > 0)
                {
                    conditions.Add("Belt = @belt");
                    parameters.Add(new SqlParameter("@belt", (CmbBeltFilter.SelectedItem as ComboBoxItem)?.Content.ToString()));
                }

                // Status filter
                if (CmbStatusFilter.SelectedIndex > 0)
                {
                    conditions.Add("StudentStatus = @status");
                    parameters.Add(new SqlParameter("@status", (CmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString()));
                }

                // For instructors, only show their students
                if (_currentUserType == "Instructor")
                {
                    conditions.Add("InstructorName = @instructorName");
                    parameters.Add(new SqlParameter("@instructorName", _currentUserName));
                }

                // Add conditions with selected logic (AND/OR)
                if (conditions.Count > 0)
                {
                    string logic = RbAnd.IsChecked == true ? " AND " : " OR ";
                    query.Append(" AND (").Append(string.Join(logic, conditions)).Append(")");
                }

                var result = _dbContext.SelectData(query.ToString(), parameters.ToArray());
                StudentsGrid.ItemsSource = result.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtNameFilter.Clear();
            TxtAgeFilter.Clear();
            TxtLocationFilter.Clear();
            CmbGenderFilter.SelectedIndex = 0;
            CmbBeltFilter.SelectedIndex = 0;
            CmbStatusFilter.SelectedIndex = 0;
            CmbAgeOperator.SelectedIndex = 0;
            RbAnd.IsChecked = true;

            LoadStudents();
        }

        private void StudentsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                var studentDetails = new StudentDetails(
                    Convert.ToInt32(row["Student_ID"]),
                    _currentUserType,
                    _currentUserName
                );
                studentDetails.ShowDialog();
                LoadStudents(); // Refresh after details window is closed
            }
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            var studentDetails = new StudentDetails(0, _currentUserType, _currentUserName); // 0 indicates new student
            if (studentDetails.ShowDialog() == true)
            {
                LoadStudents();
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                var studentDetails = new StudentDetails(
                    Convert.ToInt32(row["Student_ID"]),
                    _currentUserType,
                    _currentUserName
                );
                studentDetails.ShowDialog();
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Please select a student to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnStopStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is DataRowView row)
            {
                if (MessageBox.Show("Are you sure you want to stop this student?", "Confirm Stop", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var parameters = new SqlParameter[]
                        {
                            new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        };

                        string query = "UPDATE Student_Data SET StudentStatus = 'Stopped' WHERE Student_ID = @studentId";
                        _dbContext.UpdateData(query, parameters);
                        LoadStudents();
                        MessageBox.Show("Student status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating student status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (_currentUserType != "Admin")
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
                        var parameters = new SqlParameter[]
                        {
                            new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        };

                        string query = "DELETE FROM Student_Data WHERE Student_ID = @studentId";
                        _dbContext.DeleteData(query, parameters);
                        LoadStudents();
                        MessageBox.Show("Student deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

    }
}