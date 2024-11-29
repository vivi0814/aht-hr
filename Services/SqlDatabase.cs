using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace HR_WorkFlow.Services
{
	public class SqlDatabase : IDisposable
	{
		protected string ConnectionString { get; set; }
		protected SqlConnection? Connection { get; set; }

		public SqlDatabase(IConfiguration configuration)
		{
			ConnectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public void Open()
		{
			Connection = new SqlConnection(ConnectionString);
			Connection.Open();
		}

		public SqlCommand CreateCommand(string query)
		{
			if (Connection == null)
			{
				Open();
			}

			return new SqlCommand(query, Connection);
		}

		public void Dispose()
		{
			Connection?.Dispose();
		}

        internal DataTable ExecuteDataTable(string query, Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        internal DataTable ExecuteDataTable(string query)
        {
            throw new NotImplementedException();
        }
    }
}
