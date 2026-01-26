using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zenskar_MAMS.Helpers
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<UserTable> Users =>
            _database.GetCollection<UserTable>("Users");

        public IMongoCollection<StudentTable> Students =>
            _database.GetCollection<StudentTable>("Students");

        public IMongoCollection<AttendanceTable> Attendance =>
            _database.GetCollection<AttendanceTable>("Attendance");

        public IMongoCollection<RequestTable> Requests =>
            _database.GetCollection<RequestTable>("Requests");
    }
    public class CommonItems
    {
        public static MongoClientSettings settings = MongoClientSettings.FromConnectionString(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString);
        public static MongoClient client = new MongoClient(settings);
        public static IMongoDatabase Db = client.GetDatabase("ZenskarDB");
        public static MongoDbContext _mongoContext = new MongoDbContext(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "ZenskarDB");
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
        public static DataTable ToDataTableLong(long input)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(long));
            DataRow row = dt.NewRow();
            row["Value"] = input;
            dt.Rows.Add(row);
            return dt;
        }
    }
    
    public class LoginUser
    {
        public int User_ID { get; set; }
        public string User_Name { get; set; }
        public string User_Type { get; set; }
        public string Status { get; set; }

    }
}
