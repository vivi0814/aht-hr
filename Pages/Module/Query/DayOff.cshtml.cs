using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

namespace HR_WorkFlow.Pages.Module.Query
{
    public class DayOffModel : PageModel
    {
        [BindProperty]
        public string Time_Period { get; set; }

        [BindProperty]
        public string EMP_No { get; set; }

        [BindProperty]
        public string Arrival_Date { get; set; }

        [BindProperty]
        public string Seniority { get; set; }

        [BindProperty]
        public string PaidLeave_Hours { get; set; }

        [BindProperty]
        public string PaidLeave_Taken { get; set; }

        [BindProperty]
        public string PaidLeave_Left { get; set; }

        [BindProperty]
        public string CompTime_Hours { get; set; }

        [BindProperty]
        public string CompTime_Taken { get; set; }

        [BindProperty]
        public string CompTime_Left { get; set; }

        [BindProperty]
        public string PaidLeave_Expired { get; set; }

        [BindProperty]
        public string CompTime_Expired { get; set; }

        [BindProperty]
        public string Personal_Leave { get; set; }

        [BindProperty]
        public string Maternity_Leave { get; set; }

        [BindProperty]
        public string Parental_Leave { get; set; }

        [BindProperty]
        public string Sick_Leave { get; set; }

        [BindProperty]
        public string Marriage_Leave { get; set; }

        [BindProperty]
        public string Paternity_Leave { get; set; }

        [BindProperty]
        public string Official_Leave { get; set; }

        [BindProperty]
        public string Funeral_Leave { get; set; }

        [BindProperty]
        public string Menstruation_Leave { get; set; }

        [BindProperty]
        public DateTime? Create_Date { get; set; }

        

        protected SqlDatabase Database { get; set; }    //引用共用函式(SQLService)


        public DayOffModel(SqlDatabase database)   //注入服務(SQL&log) 
        {
            Database = database;
        }

        public IActionResult OnGet()
        {

            ViewData["QueryResult"] = QueryResult();
            return Page();
        }
        private DataTable QueryResult()
        {
            string strSQL = @"
SELECT L.Create_Date,
              E.Emp_Name,
              L.Leave_No,
              L.Start_Date,
              L.End_Date,
              L.Leave_Content
  FROM Leave_Overtime L WITH(NOLOCK)
     LEFT JOIN EMP_Data E WITH(NOLOCK) ON L.Emp_No= E.EMP_No
WHERE L.Emp_No = @EmpNo
               ";


            SqlCommand command = Database.CreateCommand(strSQL);
            var userName = User.Identity?.Name;
            command.Parameters.AddWithValue("@EmpNo", userName);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
    }
}
