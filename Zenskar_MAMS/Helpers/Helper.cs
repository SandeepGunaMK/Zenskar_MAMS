using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zenskar_MAMS.Windows;

namespace Zenskar_MAMS.Helpers
{

    public class CommonItems
    {
        public static MongoClientSettings settings = MongoClientSettings.FromConnectionString(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString);
        public static MongoClient client = new MongoClient(settings);
        public static IMongoDatabase Db = client.GetDatabase("ZenskarDB");
        public static MongoDbContext _mongoDBContext = new MongoDbContext(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "ZenskarDB");
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
        public static DataTable ToDataTable(List<UserTable> users)
        {
            var dt = new DataTable();

            dt.Columns.Add("Login_ID");
            dt.Columns.Add("User_Name");
            dt.Columns.Add("Contact_Number");
            dt.Columns.Add("User_Type");
            dt.Columns.Add("Status");
            dt.Columns.Add("Created_Date", typeof(DateTime));
            dt.Columns.Add("Approved_By");

            foreach (var u in users)
            {
                dt.Rows.Add(
                    u.Login_ID,
                    u.User_Name,
                    u.Contact_Number,
                    u.User_Type,
                    u.Status,
                    u.Created_Date,
                    u.Approved_By
                );
            }

            return dt;
        }

        public static string GetBatchID(string location, string belt, string insName)
        {
            string batchId = string.Empty;
            var filter = Builders<StudentTable>.Filter.Eq(x => x.Location, location) 
                & Builders<StudentTable>.Filter.Eq(x => x.Belt, belt)
                & Builders<StudentTable>.Filter.Eq(x => x.InstructorName, insName);
            var projection = Builders<StudentTable>.Projection.Include(x => x.Batch_ID);
            var student = CommonItems._mongoDBContext.Students.Find(filter).Project<StudentTable>(projection).FirstOrDefault();            
            batchId = student?.Batch_ID ?? string.Empty;
            if (batchId == string.Empty)
            {
                var filter1 = Builders<StudentTable>.Filter.Empty;
                var projection1 = Builders<StudentTable>.Projection.Include(x => x.Batch_ID);
                var batchIds = CommonItems._mongoDBContext.Students
                    .Find(filter1)
                    .Project<StudentTable>(projection1).ToList();
                batchIds.Sort((x, y) => string.Compare(x.Batch_ID, y.Batch_ID));
                string maxBatchId = batchIds.LastOrDefault()?.Batch_ID ?? string.Empty;
                batchId = IncrementBatchId(maxBatchId);
            }
            return batchId;
        }
        public static string IncrementBatchId(string input)
        {
            var match = Regex.Match(input, @"^(.*_)(\d+)$");

            if (!match.Success)
                throw new ArgumentException("Invalid batch format");

            string prefix = match.Groups[1].Value;   // "zen_"
            string numberPart = match.Groups[2].Value; // "001"

            int number = int.Parse(numberPart);
            number++;

            string newNumber = number.ToString().PadLeft(numberPart.Length, '0');

            return prefix + newNumber;
        }
    }
        public class LoginUser
        {
            public int User_ID { get; set; }
            public string User_Name { get; set; }
            public string User_Type { get; set; }
            public string Status { get; set; }

        }
    
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

            public IMongoCollection<LocationTable> Locations =>
                _database.GetCollection<LocationTable>("Locations");

            public IMongoCollection<BeltTable> Belts =>
                _database.GetCollection<BeltTable>("Belts");
            public IMongoCollection<RequestTable> Requests =>
                _database.GetCollection<RequestTable>("Requests");
    }
    }
