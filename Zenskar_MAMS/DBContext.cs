using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Zenskar_MAMS
{
    //public class DBContext
    //{
    //    private readonly string _connectionString;

    //    public DBContext()
    //    {
    //        _connectionString = ConfigurationManager.ConnectionStrings["ZenskarDB"].ConnectionString;
    //    }

    //    public SqlConnection CreateConnection()
    //    {
    //        return new SqlConnection(_connectionString);
    //    }

    //    public DataTable SelectData(string query, SqlParameter[] parameters = null)
    //    {
    //        using var connection = CreateConnection();
    //        using var adapter = new SqlDataAdapter(query, connection);
    //        if (parameters != null)
    //        {
    //            adapter.SelectCommand.Parameters.AddRange(parameters);
    //        }
    //        var dataTable = new DataTable();
    //        adapter.Fill(dataTable);
    //        return dataTable;
    //    }

    //    public int InsertData(string query, SqlParameter[] parameters)
    //    {
    //        using var connection = CreateConnection();
    //        using var command = new SqlCommand(query, connection);
    //        command.Parameters.AddRange(parameters);
    //        connection.Open();
    //        return command.ExecuteNonQuery();
    //    }

    //    public int UpdateData(string query, SqlParameter[] parameters)
    //    {
    //        using var connection = CreateConnection();
    //        using var command = new SqlCommand(query, connection);
    //        command.Parameters.AddRange(parameters);
    //        connection.Open();
    //        return command.ExecuteNonQuery();
    //    }

    //    public int DeleteData(string query, SqlParameter[] parameters)
    //    {
    //        using var connection = CreateConnection();
    //        using var command = new SqlCommand(query, connection);
    //        command.Parameters.AddRange(parameters);
    //        connection.Open();
    //        return command.ExecuteNonQuery();
    //    }
    //}
}