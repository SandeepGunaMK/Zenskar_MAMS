using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class MasterHome : Window
    {
        private readonly string _userName;
        private readonly DBContext _dbContext;
        private readonly string _userType;
        private DataTable _DBData;

        public MasterHome(string userName)
        {
            _dbContext = new DBContext();
            InitializeComponent();
            _userName = userName;
            LoadExamDueBatchs();
        }

        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            var studentsList = new StudentsList("Master", _userName);
            studentsList.ShowDialog();
        }

        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var requestsWindow = new RequestsWindow("Master", _userName);
            requestsWindow.ShowDialog();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutHelper.Logout(this); OnClosed(e);
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
                    " where MasterName = '@master' " +
                    "AND StudentStatus = 'Active'";
                var parameter = new SqlParameter[] { new("@master", _userName) };
                _DBData = _dbContext.SelectData(query, parameter);
                DataRow[] examDueStudents = _DBData.Select("ExamDue = 'Yes'");
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