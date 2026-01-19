using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zenskar_MAMS.Helpers
{
    public class common
    {
        public static MongoClientSettings settings = MongoClientSettings.FromConnectionString(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString);
        public static MongoClient client = new MongoClient(settings);
        public static IMongoDatabase Db = client.GetDatabase("ZenskarDB");
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dt = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                Type colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dt.Columns.Add(prop.Name, colType);
            }

            foreach (var item in items)
            {
                DataRow row = dt.NewRow();
                foreach (var prop in props)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }
    }
    
    public class LoginUser1
    {
        public int User_ID { get; set; }
        public string User_Name { get; set; }
        public string User_Type { get; set; }
        public string Status { get; set; }

    }
}
