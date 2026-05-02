using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System;
using System.Data;
using System.Text;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class MasterHome : Window
    {
        private readonly string _userName;
        //private readonly DBContext _dbContext;
        private readonly string _userType;
        private DataTable _DBData;

        // Static flag to track if message has been shown in this login session
        public static bool _examDueMessageShown = false;

        public MasterHome(string userName)
        {
            //_dbContext = new DBContext();
            InitializeComponent();
            _userName = userName;

            LoadExamDueBatchs();
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new ChangePasswordWindow();
                dlg.Owner = this;
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Change Password dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnStudentsList_Click(object sender, RoutedEventArgs e)
        {
            var studentsList = new StudentsList("Master", _userName);
            this.Close();
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
                #region OldQuery
                //string condition = "CASE \r\n" +
                //    "WHEN Belt = 'White' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 45 THEN 'Yes'\r\n" +
                //    "WHEN Belt = 'White Senior' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 60 THEN 'Yes'\r\n" +
                //    "WHEN Belt = 'Yellow' \r\n AND DATEDIFF(DAY, LastExamDate, GETDATE()) > 90 THEN 'Yes'\r\n" +
                //    "ELSE 'No'" +
                //    "END AS 'ExamDue' ";

                //string query = "SELECT Distinct Location,Belt,InstructorName,MasterName, " + condition + " FROM Student_Data" +
                //    " where MasterName = '@master' " +
                //    "AND StudentStatus = 'Active'";
                //var parameter = new SqlParameter[] { new("@master", _userName) };
                //_DBData = _dbContext.SelectData(query, parameter);
                #endregion
                #region MongoDb
                var filter = Builders<StudentTable>.Filter.Eq(x => x.MasterName, _userName) & Builders<StudentTable>.Filter.Eq(x => x.StudentStatus, "Active");
                var list = CommonItems._mongoDBContext.Students
                            .Find(filter)
                            .Project(x => new
                            {
                                x.Location,
                                x.Belt,
                                x.InstructorName,
                                x.MasterName,
                                x.LastExamDate
                            })
                            .ToList();
                var result = list.Select(x =>
                {
                    string examDue = "No";

                    if (x.LastExamDate != null && !string.IsNullOrEmpty(x.Belt))
                    {
                        int days = (DateTime.Now - x.LastExamDate.Value).Days;

                        bool due =
                            (x.Belt == "White" && days > 45) ||
                            (x.Belt == "White Senior" && days > 60) ||
                            (x.Belt == "Yellow" && days > 90);

                        examDue = due ? "Yes" : "No";
                    }

                    return new
                    {
                        x.Location,
                        x.Belt,
                        x.InstructorName,
                        x.MasterName,
                        ExamDue = examDue
                    };
                })
                .DistinctBy(x => new
                {
                    x.Location,
                    x.Belt,
                    x.InstructorName,
                    x.MasterName,
                    x.ExamDue
                }).ToList();
                _DBData = CommonItems.ToDataTable(result);
                #endregion

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
            // Only show message box if it hasn't been shown in this login session
            if (_examDueMessageShown)
            {
                return;
            }

            // Mark that message has been shown in this session
            _examDueMessageShown = true;

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