using HR_WorkFlow.Pages;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.ExceptionServices;
using System.Security.Claims;

namespace HR_WorkFlow.Services
{
    public class MenuBase
    {
        private SqlDatabase Database { get; set; }    //引用共用函式(SQLService)
        private IHttpContextAccessor HttpContextAccessor { get; set; } 

        public MenuBase(SqlDatabase database, IHttpContextAccessor httpContextAccessor)   //注入服務(SQL&log)
        {
            Database = database;
            HttpContextAccessor = httpContextAccessor;
        }

        public Dictionary<string, Dictionary<string, string>> GetMenu()
        {
            string query = @"SELECT * 
                               FROM dbo.Tran_View WITH(NOLOCK)
                              WHERE GPRole <= @Role
                                AND STRole <= @Role";
            SqlCommand command = Database.CreateCommand(query);
            string userRole = HttpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Role)!.Value;
            command.Parameters.AddWithValue("@Role", userRole);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);

            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
            string FirstMenu = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                if (FirstMenu!=row["Group_Name"].ToString())
                { 
                    result.Add(row["Group_Name"].ToString()!, new Dictionary<string, string>());
                    FirstMenu = row["Group_Name"].ToString()!;
                }
                result[FirstMenu].Add(row["Tran_Name"].ToString()!, row["Tran_Url"].ToString()!);
            }
            return result;
        }
    }
}