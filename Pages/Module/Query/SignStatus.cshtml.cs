using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace HR_WorkFlow.Pages.Module.Query
{
    public class SignStatusModel : PageModel
    {
        private object dt;

        [BindProperty]
        public string? Start_Time { get; set; }

        [BindProperty]
        public string? End_Time { get; set; }

        [BindProperty]
        //public string? Status { get; set; }

        protected SqlDatabase? Database { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        private int PageSize { get; } = 10;

        public SignStatusModel(SqlDatabase database)   //注入服務(SQL&log) 
        {
            Database = database;
        }

        protected Common? Common { get; set; }

        [BindProperty]
        public DataTable SignStatus { get; set; }

        public int TotalPages { get; set; }

        //Step1
        //取得目前使用者代號
        //SQL 加入判斷條件 僅帶出=目前使用者的資訊
        //
        //Step2
        //根據下拉式選單 給予 參數 執行判斷
        // 空白=全部
        // 未提交1
        // 簽核中2
        // 已審核3
        //   駁回4


        public IActionResult OnPostQuery()
        {
            string query = @"SELECT
            Leave_No,
            Emp_No,
            Sign_No,
            S2.Code_Name AS Leave_Name,
            Start_Date,
            End_Date,
            Hours,
            Leave_Content,
            S1.Code_Name AS Status_Name
            FROM Leave_Overtime AS L WITH(NOLOCK)
            LEFT JOIN DDL_Setting AS D1 WITH(NOLOCK) ON L.Leave = D1.Code_Sort AND D1.Code_No = 'F01'
            LEFT JOIN DDL_Setting AS D2 WITH(NOLOCK) ON L.Status = D2.Code_No AND D2.Code_Name = 'L01'
            LEFT JOIN DDL_Setting AS S1 WITH(NOLOCK) ON L.Leave = S1.Code_Sort AND S1.DDL_No = 'F01'
            LEFT JOIN DDL_Setting AS S2 WITH(NOLOCK) ON L.Status = S2.Code_No AND S2.DDL_No = 'L01'
            WHERE L.Start_Date >= @Start_Time AND L.End_Date <= @End_Time";

            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Start_Time", Start_Time);
            command.Parameters.AddWithValue("@End_Time", End_Time);


            SignStatus = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(SignStatus);
            ViewData["QueryResult"] = SignStatus;

            return Page();
        }
        //public void OnPostExport()
        //{
        //    TempData["Action"] = "Add";
        //}

    }
}