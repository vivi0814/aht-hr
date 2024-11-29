using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static HR_WorkFlow.Pages.Module.Form.LeaveModel;
using DataTable = System.Data.DataTable;

namespace HR_WorkFlow.Pages.Module.Query
{
    [Authorize]
    public class QueryClassScheduleModel : PageModel
    {

        protected SqlDatabase Database { get; set; }

        //List<CalendarData> eventsList = new List<CalendarData>();

        public QueryClassScheduleModel(SqlDatabase database)
        {
            Database = database;

        }
        public class CalendarDataRequest
        {
            public string Emp_No { get; set; }
            public DateTime HR_Day { get; set; }
            public string DayNight { get; set; }
            public string Type { get; set; }
            public string Shift { get; set; } // 添加 '班表' 屬性
        }

        public JsonResult OnPostCalendarData([FromBody] CalendarDataRequest requestModel)
        {

            try
            {
                string strSQL = @"
            SELECT HR_DAY,
                CASE
                    WHEN TYPE = 0 AND DAYNIGHT = 0 THEN '早班'
                    WHEN TYPE = 0 AND DAYNIGHT = 1 THEN '晚班'
                    WHEN TYPE = 0 AND DAYNIGHT = 2 THEN '中班'
                    WHEN DAYNIGHT = 3 AND TYPE = 1 THEN '休息日'
                    WHEN DAYNIGHT = 3 AND TYPE = 2 THEN '例假'
                    WHEN DAYNIGHT = 3 AND TYPE = 3 THEN '國定假日'
                    ELSE '異常'
                END AS Shift
            FROM
                HelpDesk WITH(NOLOCK)
            WHERE EMP_No = @EmpNo;
        ";
                var userName = User.Identity?.Name;

                SqlCommand command = Database.CreateCommand(strSQL);
                command.Parameters.AddWithValue("@EmpNo", userName);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dt);

                
                string jsonResult = ConvertDataTableToJson(dt);
                return new JsonResult(jsonResult);
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return new JsonResult($"Error: {ex.Message}");
            }
        }
        private string ConvertDataTableToJson(DataTable dataTable)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (col.ColumnName == "HR_DAY" && row[col] is DateTime)
                    {
                        // 將日期轉換為字串格式，只保留日期部分
                        dict[col.ColumnName] = ((DateTime)row[col]).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        dict[col.ColumnName] = row[col];
                    }
                }
                rows.Add(dict);
            }

            return JsonConvert.SerializeObject(rows);
        }


    }
}