using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Demo.DataAccess
{
    public class SqlServer
    {
        private readonly SqlConnection sqlconnection;

        /// <summary>
        /// Crea una instancia de la clase que conecta a la BD
        /// </summary>
        /// <param name="connectionString">Cadena de conexión</param>
        public SqlServer(string connectionString)
        {
            sqlconnection = new SqlConnection(connectionString);
        }

        public SqlServer(string server, string databaseName)
        {
            string connString = $"data source={server};initial catalog={databaseName};integrated security=true";
            sqlconnection = new SqlConnection(connString);
        }

        public SqlServer(string server, string databaseName, string username, string password)
        {
            string connString = $"data source={server};initial catalog={databaseName};username={username};password={password}";
            sqlconnection = new SqlConnection(connString);
        }

        public int ExecuteQuery(string query)
        {
            try
            {
                OpenConnection();
                var cmd = new SqlCommand(query, sqlconnection);
                int result = cmd.ExecuteNonQuery();
                sqlconnection.Close();
                sqlconnection.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlconnection.State != ConnectionState.Closed)
                {
                    sqlconnection.Close();
                    sqlconnection.Dispose();
                }
            }
        }

        public void OpenConnection()
        {
            sqlconnection = new SqlConnection(connectionString);
            if (sqlconnection.State != ConnectionState.Open)
                sqlconnection.Open();
        }

        public DataTable ExecuteQueryTable(string query)
        {
            try
            {
                OpenConnection();
                var cmd = new SqlCommand(query, sqlconnection);
                var adp = new SqlDataAdapter(cmd);
                var dtResult = new DataTable();
                adp.Fill(dtResult);
                sqlconnection.Close();
                sqlconnection.Dispose();
                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlconnection.State != ConnectionState.Closed)
                {
                    sqlconnection.Close();
                    sqlconnection.Dispose();
                }
            }
        }

        public DataTable GetStoreProcedureTable(string spName, Dictionary<string, object> parameters = null)
        {
            try
            {
                OpenConnection();
                var cmd = new SqlCommand(spName, sqlconnection);
                cmd.CommandType = CommandType.StoredProcedure;

                if(parameters != null)
                {
                    var parameterList = GetSqlParameters(parameters);
                    cmd.Parameters.AddRange(parameterList.ToArray());
                }

                var adp = new SqlDataAdapter(cmd);
                var dtResult = new DataTable();
                adp.Fill(dtResult);
                sqlconnection.Close();
                sqlconnection.Dispose();
                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlconnection.State != ConnectionState.Closed)
                {
                    sqlconnection.Close();
                    sqlconnection.Dispose();
                }
            }
        }

        private List<SqlParameter> GetSqlParameters(Dictionary<string, object> parameters)
        {
            var parameterList = new List<SqlParameter>();
            foreach (var paramKey in parameters.Keys)
            {
                object paramValue = null;
                if (parameters.TryGetValue(paramKey, out paramValue))
                    parameterList.Add(new SqlParameter()
                    {
                        ParameterName = paramKey,
                        SqlValue = paramValue
                    });
            }
            return parameterList;
        }

        public List<T> GetFromStoreProcedureAsObjects<T>(string spName, Dictionary<string, object> parameters = null) where T : class, new()
        {
            var dt = GetStoreProcedureTable(spName, parameters);
            var result = GetEntityList<T>(dt);
            return result;
        }

        #region Reflected Methods
        private List<T> GetEntityList<T>(DataTable data) where T : class, new()
        {
            List<T> result = new List<T>();

            if (data == null)
            {
                return result;
            }

            if (data.Rows.Count == 0)
            {
                return result;
            }

            foreach (DataRow row in data.Rows)
            {
                T newInstance = new T();
                LoadEntityFromDataRow<T>(newInstance, row);
                result.Add(newInstance);
            }

            return result;
        }

        /// <summary>
        /// Obtiene un objeto cargado con base en un dataRow
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="targetEntity"></param>
        /// <param name="data"></param>
        private void LoadEntityFromDataRow<T1>(T1 targetEntity, DataRow data)
        {
            foreach (var property in targetEntity.GetType().GetProperties())
            {
                if (property != null && property.CanWrite)
                {
                    if (data.Table.Columns.Contains(property.Name))
                    {
                        if (data[property.Name] != System.DBNull.Value)
                        {
                            object propertyValue = null;

                            if (property.PropertyType == typeof(DateTime?))
                                propertyValue = Convert.ToDateTime(data[property.Name]);
                            else if (property.PropertyType == typeof(int?))
                                propertyValue = Convert.ToInt32(data[property.Name]);
                            else if (property.PropertyType.BaseType.FullName.Equals("System.Enum"))
                            {
                                propertyValue = (object)System.Enum.Parse(property.PropertyType, data[property.Name].ToString());
                            }
                            else
                                propertyValue = System.Convert.ChangeType(
                                       data[property.Name],
                                       property.PropertyType
                                   );
                            property.SetValue(targetEntity, propertyValue, null);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
