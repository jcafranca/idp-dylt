

using Dapper;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;
using static DDCInteligence.Utilities.Helpers.SimpleCRUD;

namespace DDCInteligence.Utilities.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }

    public class MySQLServices
    {
        private readonly LogServices log = new LogServices();
        public dynamic GetConnectionString(dynamic databaseConnection, bool isLookup = false) { 
            if (databaseConnection != null) { 
                if (databaseConnection is string conn) return new MySqlConnection(conn); 
                if (databaseConnection is Settings databaseConn && databaseConn.Server.Count > 0) {
                    foreach (var item in databaseConn.Server) {

                        var srv = $"server={item};port={databaseConnection.Port};user={databaseConnection.Username};password={databaseConnection.Password};database={databaseConnection.Databases[0]};sslmode=None"; 
                        if (isLookup)
                        {
                            srv = $"server={item};port={databaseConnection.Port};user={databaseConnection.Username};password={databaseConnection.Password};database={databaseConnection.Databases[1]};sslmode=None";
                        }
                        
                        using (IDbConnection connection = new MySqlConnection(srv)) {
                            try { 
                                connection.Open(); 
                                if (connection.State == ConnectionState.Open)
                                    return srv;
                            } catch (MySqlException ex) { 
                                log.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetConnectionString)); 
                            }
                        }
                    }
                }
            }
            return null;
        }
        public int CreateRequest(string connection_string, RequestInfo request)
        {
            using (IDbConnection connection = new MySqlConnection(connection_string)) 
            {
                string command = $"INSERT INTO request_tbl (user_id, doc_id, image_path, request_type) VALUES (@user_id, @doc_id, @image_path, @request_type); SELECT LAST_INSERT_ID()";
                return connection.QueryFirstOrDefault<int>(command, request);
            }
        }
        public RequestInfo GetRequestInfoById(string connection_string, int _id)
        {
            using (IDbConnection connection = new MySqlConnection(connection_string))
            {
                string command = $"SELECT * FROM request_tbl WHERE id = {_id};";
                return connection.QueryFirstOrDefault<RequestInfo>(command);
            }
        }
        public RequestInfo GetRequestInfo(string connection_string, Dictionary<string, object> conditions)
        {
            using (IDbConnection connection = new MySqlConnection(connection_string))
            {
                var where_conditions = string.Join(" AND ", conditions.Select(kv => $"{kv.Key} = '{kv.Value}'"));
                string command = $"SELECT * FROM request_tbl WHERE {where_conditions};";
                return connection.QueryFirstOrDefault<RequestInfo>(command);
            }
        }
        public int Insert<T>(T any, string connectionString)
        {
            using (IDbConnection connection = new MySqlConnection(connectionString))
            {
                if (any != null && any is ActivityInfo act)
                {
                    return connection.Insert<ActivityInfo>(act).Value;
                }
            }

            return 0;
        }
        public  async Task<IEnumerable<T>> GetQueryAsync<T>(string connectionString, Dictionary<string, object> where = null, IEnumerable<string> likeColumn = null)
        {
            var tableName = GetTableName(typeof(T));
            using (var connection = new MySqlConnection(connectionString))
            {
                string where_conditions = "";
                string query_builder = $"SELECT * FROM {tableName} ";
                if (where != null && where.Count > 0)
                {
                   where_conditions = string.Join(" AND ", where.Select(kv =>
                   {

                       if (likeColumn != null && likeColumn.Contains(kv.Key))
                       {
                           return $"{kv.Key} LIKE '%{kv.Value}%'";
                       }

                       return $"{kv.Key} = '{kv.Value}'";

                   }));
                   query_builder += $" WHERE {where_conditions};";
                }

                var result = await connection.QueryAsync<T>(query_builder); // ✅ Fully async
                return result;
            }
        }
        public async Task<T> GetQueryAsyncByClass<T>(string connectionString, Dictionary<string, object> param)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string tableName = GetTableName(typeof(T));

                // Assume only one key-value pair in the dictionary
                var key = param.Keys.First();
                var value = param[key];

                string query = $"SELECT * FROM {tableName} WHERE `{key}` = @{key} LIMIT 1;";
                var parameters = new DynamicParameters();
                parameters.Add($"@{key}", value);

                var result = await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
                return result;
            }
        }
    }
}
