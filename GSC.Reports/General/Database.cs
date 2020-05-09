using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Reports.General
{
    public static class Database
    {
        #region Parameters Management

        private static List<SqlParameter> parameters;

        public static void ResetParameters()
        {
            parameters = new List<SqlParameter>();
        }

        public static void AddParam(string name, object value)
        {
            parameters.Add(new SqlParameter(name, value));
        }

        #endregion

        private static SqlConnection OpenConnection()
        {
            //string connStr1 = ConfigurationManager.AppSettings["dbConnectionString"];
            //string connStr2 = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;

            var connStr = "Data Source=BHAVIK\\SQLEXPRESS;Initial Catalog=GSC;user id=sa;password=admin123";

            var sqlConnection = new SqlConnection(connStr);
            sqlConnection.Open();

            return sqlConnection;
        }

        public static DataSet ExecuteSql(string strSql)
        {
            SqlCommand sqlCommand = null;
            SqlConnection sqlConnection = null;
            SqlDataAdapter sqlDataAdapter = null;
            try
            {
                sqlConnection = OpenConnection();
                sqlCommand = new SqlCommand
                {
                    CommandText = strSql,
                    CommandType = CommandType.Text,
                    Connection = sqlConnection
                };

                if (parameters != null && parameters.Count > 0)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }

                DataSet dsReturn = new DataSet();
                sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dsReturn);
                return dsReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
                if (sqlDataAdapter != null)
                    sqlDataAdapter.Dispose();
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }

        public static object ExecuteScalar(string strSql)
        {
            SqlCommand sqlCommand = null;
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = OpenConnection();
                sqlCommand = new SqlCommand
                {
                    CommandText = strSql,
                    CommandType = CommandType.Text,
                    Connection = sqlConnection
                };

                if (parameters != null && parameters.Count > 0)
                {
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                }
                return sqlCommand.ExecuteScalar(); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }
    }
}
