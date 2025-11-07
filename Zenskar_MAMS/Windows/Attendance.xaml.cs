using Microsoft.VisualBasic;
using Azure;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Zenskar_MAMS.Windows
{
    /// <summary>
    /// Interaction logic for Attendance.xaml
    /// </summary>
    public partial class Attendance : Window
    {
        private readonly DBContext _dbContext;
        private readonly int _studentId;
        private readonly string _currentUserType;
        private readonly string _currentUserName;
        private string _StudentName;
        private string _AttendedClasses;
        private string _TotalClasses;
        private string _year;
        //private string _yearDisplay;
        //public string YearDisplay
        //{
        //    get => _yearDisplay;
        //    set
        //    {
        //        _yearDisplay = value;
        //        OnPropertyChanged(nameof(YearDisplay));
        //    }
        //}
        public Attendance(int studentId, string userType, string userName)
        {
            InitializeComponent();
            this.DataContext = this;
            _dbContext = new DBContext();
            _studentId = studentId;
            _currentUserType = userType;
            _currentUserName = userName;



            LoadAttendanceData();
        }
        public Attendance()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            this.Close();
        }
        private void RefreshMonthData_Click(object sender, RoutedEventArgs e)
        {
            LoadAttendanceData();
        }
        private void UpdateNoofClassesAttended(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = Interaction.InputBox("Enter No Of Classes Attended", "Attendance", _AttendedClasses);
                if (input == "") { MessageBox.Show("Input cannot be empty", "Attendance"); return; }
                else if (int.Parse(input) > int.Parse(_TotalClasses))
                {
                    MessageBox.Show("Attended classes cannot be more than total classes", "Attendance");
                    return;
                }
                else { _AttendedClasses = input; }

                string updateValue = _AttendedClasses + "-" + _TotalClasses + "-" + _year;
                string query = "UPDATE Attendance SET " +
                               (Month.SelectedItem as ComboBoxItem)?.Content?.ToString() +
                               " = '" + updateValue +
                               "' WHERE Student_ID = @studentId";
                var parameters = new SqlParameter[]
                            {
                            new("@studentId", _studentId)
                            };
                _dbContext.UpdateData(query, parameters);
                LoadAttendanceData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attendance");
            }
        }

        private void UpdateTotalNoofClasses(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = Interaction.InputBox("Enter No Of Classes Attended", "Attendance", _TotalClasses);
                if (int.Parse(input)>1){ }
                if (input == "") { MessageBox.Show("Input cannot be empty", "Attendance"); return; }
                else { _TotalClasses = input; }

                string updateValue = _AttendedClasses + "-" + _TotalClasses + "-" + _year;
                string query = "UPDATE Attendance SET " +
                               (Month.SelectedItem as ComboBoxItem)?.Content?.ToString() +
                               " = '" + updateValue +
                               "' WHERE Student_ID = @studentId";
                var parameters = new SqlParameter[]
                            {
                            new("@studentId", _studentId)
                            };
                _dbContext.UpdateData(query, parameters);
                LoadAttendanceData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attendance");
            }
        }

        private void LoadAttendanceData()
        {
            try
            {
                //string monthFilter;
                string query;
                string? SelectedMonth = (Month.SelectedItem as ComboBoxItem)?.Content?.ToString();
                var parameters = new SqlParameter[] { new("@studentId", _studentId) };
                if (SelectedMonth == "All" || SelectedMonth == null) { query = "SELECT * FROM Attendance WHERE Student_ID = @studentId"; }
                else { query = "SELECT Name, " + SelectedMonth + " FROM Attendance WHERE Student_ID = @studentId";}

                var result = _dbContext.SelectData(query, parameters);
                if (result != null)
                {
                    _StudentName = result.Rows[0]["Name"].ToString();
                    string data = result.Rows[0][SelectedMonth].ToString();
                    string[] parts = data.Split('-');
                    if (parts.Length == 3)
                    {
                        _AttendedClasses = AttendedClassesDisplay.Text = parts[0];
                        _TotalClasses = TotalClassesDisplay.Text = parts[1];
                        _year = TxtYearDisplay.Text = parts[2];
                    }
                }
                else {
                    MessageBox.Show("No attendance data found for the selected month.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error loading student data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
