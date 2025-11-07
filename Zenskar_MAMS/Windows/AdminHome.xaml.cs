using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class AdminHome : Window
    {
        private readonly string _userName;
        private readonly DBContext _dbContext;
        private readonly string _userType;
        private DataTable _DBData;

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 1)
            {
                Application.Current.Shutdown();
            }
        }
        public AdminHome(string userName = "Admin")
        {
            DataContext = this;
            _dbContext = new DBContext();
            InitializeComponent();
            _userName = userName;
            LoadExamDueBatchs();
        }

        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            var studentsList = new StudentsList("Admin", _userName);
            this.Close();
            studentsList.ShowDialog();
        }
        private void BtnAttendance_Click(object sender, RoutedEventArgs e)
        {
            var attendance = new Attendance();
            this.Close();
            attendance.ShowDialog();
        }

        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            var requestsWindow = new RequestsWindow("Admin", _userName);
            this.Close();
            requestsWindow.ShowDialog();
        }

        private void BtnManageUsers_Click(object sender, RoutedEventArgs e)
        {
            var userManagement = new UserManagement();
            this.Close();
            userManagement.ShowDialog();
        }

        private void CallLoadExamDueBatchs(object sender, RoutedEventArgs e)
        {
            LoadExamDueBatchs();
        }
        private void LoadExamDueBatchs()
        {
            try
            {
                string condition = "CASE \r\n" +
                    "WHEN Belt = 'White' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 45 THEN 'Yes'\r\n" +
                    "WHEN Belt = 'White Senior' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 60 THEN 'Yes'\r\n" +
                    "WHEN Belt = 'Yellow' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 90 THEN 'Yes'\r\n" +
                    "ELSE 'No'" +
                    "END AS 'ExamDue' ";

                string query = "SELECT Distinct Location,Belt,InstructorName,MasterName, " + condition + " FROM Student_Data" +
                    " where StudentStatus = 'Active'";
                _DBData = _dbContext.SelectData(query);
                DataRow[] examDueStudents = _DBData.Select("ExamDue = 'Yes'");
                ExamDueCountText.Text = $"Exam Due Batches: {examDueStudents.Length}";
                DisplayExamDueBatchs(examDueStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutHelper.Logout(this);
        }
        private void DisplayExamDueBatchs(DataRow[] examDueStudents)
        {
            if (examDueStudents == null || examDueStudents.Length == 0)
            {
                MessageBox.Show("No students are due for exams.", "Exam Due Batches",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Build a string that looks like a small table
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Location | Belt | Instructor | Master");
            sb.AppendLine(new string('-', 60));

            foreach (var student in examDueStudents)
            {
                string location = student["Location"].ToString();
                string belt = student["Belt"].ToString();
                string instructor = student["InstructorName"].ToString();
                string master = student["MasterName"].ToString();

                sb.AppendLine($"{location,-10} | {belt,-12} | {instructor,-15} | {master,-15}");
            }

            MessageBox.Show(sb.ToString(), "Exam Due Batches", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}