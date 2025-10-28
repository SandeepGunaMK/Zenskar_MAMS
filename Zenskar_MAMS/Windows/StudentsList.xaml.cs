using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Zenskar_MAMS.Windows
{
    public class BatchItem
    {
        public string Location { get; set; }
        public string Batch { get; set; }
    }
    public partial class StudentsList : Window
    {
        private readonly DBContext _dbContext;
        private readonly string _userType;
        private readonly string _userName;
        private DataTable _originalData;
        private Dictionary<string, HashSet<string>> _columnFilters;
        private ICollectionView _studentsView;

        #region Dropdown Filter Properties
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

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            _dbContext = new DBContext();
            _userType = userType;
            _userName = userName;
            _columnFilters = new Dictionary<string, HashSet<string>>();

            LoadStudents();
            
            ConfigureUserPermissions();
        }
        
        private void LoadStudents()
        {
            try
            {
                string query = "SELECT * FROM Student_Data";
                
                var parameters = _userType == "Instructor" 
                    ? new SqlParameter[] { new("@userName", _userName) }
                    : Array.Empty<SqlParameter>();

                _originalData = _dbContext.SelectData(query, parameters);
                StudentsGrid.ItemsSource = _originalData.DefaultView;

                // Set alternating row colors
                StudentsGrid.AlternationCount = 2;
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

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        private void BatchFilterSelected(object sender, RoutedEventArgs e)
        {
            DataTable dt = _originalData;
            DataRow[] filteredRows = dt.Select();
            string filter = "1=1"; // always true, helps build conditions easily

            if (SelectedLocation != "All")
                filter += $" AND Location = '{SelectedLocation}'";

            if (SelectedBatch != "All")
                filter += $" AND Belt = '{SelectedBatch}'";

            if (SelectedStatus != "All")
                filter += $" AND StudentStatus = '{SelectedStatus}'";

            string? selectedAge = (Age.SelectedItem as ComboBoxItem)?.Content?.ToString();
            //if (selectedAge != "All")
            //    filter += $" AND Age " + selectedAge + "'" + AgeValue.Text + "'";
            if (string.IsNullOrEmpty(selectedAge) || selectedAge == "All") { filteredRows = dt.Select(); }
            else if (string.IsNullOrEmpty(AgeValue.Text)) { filteredRows = dt.Select(); }
            else { filter += $" AND Age " + selectedAge + "'" + AgeValue.Text + "'"; }
            
            filteredRows = dt.Select(filter);

            if (filteredRows.Length == 0)
            {
                StudentsGrid.ItemsSource = dt.Select("0=1");
                return;
            }
            StudentsGrid.ItemsSource = filteredRows.CopyToDataTable().DefaultView;
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
                        var parameterSelect = new SqlParameter[]
                        {
                            new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        };
                        var parameters = new SqlParameter[]
                        {
                            new("@studentId", Convert.ToInt32(row["Student_ID"]))
                        };

                        string query = "SELECT StudentStatus FROM Student_Data WHERE Student_ID = @studentId";
                        var result = _dbContext.SelectData(query, parameterSelect);

                        string status = result.Rows[0]["StudentStatus"].ToString();

                        if (status == "Active")
                        {
                            query = "UPDATE Student_Data SET StudentStatus = 'Stopped' WHERE Student_ID = @studentId";
                            _dbContext.UpdateData(query, parameters);
                            MessageBox.Show("Student status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if(status == "Stopped")
                        {
                            query = "UPDATE Student_Data SET StudentStatus = 'Active' WHERE Student_ID = @studentId";
                            _dbContext.UpdateData(query, parameters);
                            MessageBox.Show("Student status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else { 
                            MessageBox.Show("Student status is neither Active nor Stopped. No changes made.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        LoadStudents();
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
            BatchFilterSelected(sender, e);
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