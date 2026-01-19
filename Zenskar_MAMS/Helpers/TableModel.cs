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
        public string login_ID { get; set; }
        public string User_Name { get; set; }
        public string Contact_Number { get; set; }
        public string Password { get; set; }
        public string User_Type { get; set; }
        public string Status { get; set; }
        public DateTime Created_Date { get; set; }
        public string Approved_By { get; set; }
        public DateTime Approved_Date { get; set; }
    }
}
