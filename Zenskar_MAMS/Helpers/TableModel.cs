using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zenskar_MAMS.Helpers
{
    public class UserTable
    {
        public int User_ID { get; set; }
        public string Login_ID { get; set; }
        public string User_Name { get; set; }
        public string Contact_Number { get; set; }
        public string Password { get; set; }
        public string User_Type { get; set; }
        public string Status { get; set; }
        public DateTime Created_Date { get; set; }
        public string Approved_By { get; set; }
        public DateTime Approved_Date { get; set; }
    }

    public class StudentTable
    {
        public int Student_ID { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Location { get; set; }
        public string Belt { get; set; }
        public string InstructorName { get; set; }
        public string MasterName { get; set; }
        public string ContactNumber { get; set; }
        public string ParentsName { get; set; }
        public string MedicalConditions { get; set; }
        public DateTime LastExamDate { get; set; }
        public int Attempts { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string Comments { get; set; }
        public string StudentStatus { get; set; }
    }

    public class AttendanceTable
    {
        public string Student_ID { get; set; }
        public string Name { get; set; }
        public string January { get; set; }
        public string February { get; set; }
        public string March { get; set; }
        public string April { get; set; }
        public string May { get; set; }
        public string June { get; set; }
        public string July { get; set; }
        public string August { get; set; }
        public string September { get; set; }
        public string October { get; set; }
        public string November { get; set; }
        public string December { get; set; }
    }

    public class RequestTable
    {
        public int Request_ID { get; set; }
        public string RequestType { get; set; }
        public string RequestedBy { get; set; }
        public int Student_ID { get; set; }
        public string Status { get; set; }
        public string RejectedReason { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string UpdatedData { get; set; }
    }

}
