using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;


namespace HR_WorkFlow.Pages.Module.Query
{
    public class AttendanceModel : PageModel
    {
        private object dt;

        [BindProperty]
        public string? Start_Time { get; set; }

        [BindProperty]
        public string? End_Time { get; set; }

        [BindProperty]
        public string? Status { get; set; }

        [BindProperty]
        public int Current_Page { get; set; }

        [BindProperty]
        public DataTable Absence_Table { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;



        protected SqlDatabase? Database { get; set; }

        private readonly Excel _excel;


        public AttendanceModel(SqlDatabase database, Excel excel)   //注入服務(SQL&log) 
        {
            Database = database;
            _excel = excel;
        }




        private int PageSize { get; } = 10;
        public int TotalPages { get; set; }



        protected Common? Common { get; set; }

        private string strSQL()
        {
            string StrSQL = @"
                               SELECT CONVERT(DATE, Absence.Punch_In_Time) AS Date,
                                      Absence.Punch_In_Time,
                                      Absence.Punch_Out_Time,
                                      Absence.Working_Hours,
                                      DDLQ01.Code_name AS Attendance_Status,
		                              DDLQ02.Code_name AS Attendance_Condition
                                 FROM Absence with(nolock)
                            LEFT JOIN DDL_Setting DDLQ01 ON DDLQ01.Code_No = Absence.Attendance_Status and DDLQ01.DDL_No ='Q01'
                            LEFT JOIN DDL_Setting DDLQ02 ON DDLQ02.Code_No = Absence.Attendance_Condition and DDLQ02.DDL_No ='Q02'                               
                                WHERE EMP_No = @Username
                                  AND Convert(Date, Punch_In_Time) >= @Start_Time
                                  AND Convert(Date, Punch_Out_Time) <= @End_Time
                                  AND Attendance_Status = @Status";

            return StrSQL;
        }

        private int page = 1;


        public void OnGet()
        {
            Current_Page = 1;
        }
        public IActionResult OnPostQuery()
        {

            DateTime? StartDateTime = null;
            DateTime? EndDateTime = null;

            if (!string.IsNullOrEmpty(Start_Time))
            {
                StartDateTime = DateTime.Parse(Start_Time);
            }

            if (!string.IsNullOrEmpty(End_Time))
            {
                EndDateTime = DateTime.Parse(End_Time);
            }

            if (string.IsNullOrEmpty(Start_Time) || string.IsNullOrEmpty(End_Time))
            {
                TempData["AlertMessage"] = "請輸入起訖時間";
                return Page();
            }
            if (EndDateTime < StartDateTime)
            {
                TempData["AlertMessage"] = "結束日期不能早於起始日期";
                return Page();
            }
            if (string.IsNullOrEmpty(Status) || string.IsNullOrEmpty(Status))
            {
                TempData["AlertMessage"] = "狀態";
                return Page();
            }


            string query = strSQL();
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Username", User.Identity?.Name);
            command.Parameters.AddWithValue("@Start_Time", Start_Time);
            command.Parameters.AddWithValue("@End_Time", End_Time);
            command.Parameters.AddWithValue("@Status", Status);

            Absence_Table = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(Absence_Table);
            ViewData["QueryResult"] = Absence_Table;

            return Page();
        }




        public IActionResult OnPostExport()
        {
            string query = strSQL();
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Username", User.Identity?.Name);
            command.Parameters.AddWithValue("@Start_Time", Start_Time);
            command.Parameters.AddWithValue("@End_Time", End_Time);
            command.Parameters.AddWithValue("@Status", Status);

            Absence_Table = new DataTable();
            SqlDataAdapter Absence_da = new SqlDataAdapter(command);
            Absence_da.Fill(Absence_Table);


            DataSet Absence_ds = new DataSet();
            Absence_ds.Tables.Add(Absence_Table);

            _excel.ExportToExcel(Absence_ds);

            return Page();

        }




    }
}