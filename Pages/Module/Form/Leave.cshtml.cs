using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;


namespace HR_WorkFlow.Pages.Module.Form
{
    [Authorize]
    public class LeaveModel : PageModel
    {
        [BindProperty]
        public String? Leave_No { get; set; }

        [BindProperty]
        public String? Leave { get; set; }
        [BindProperty]
        public DateTime? Start_Date { get; set; }

        [BindProperty]
        public DateTime? End_Date { get; set; }

        [BindProperty]
        public String? Leave_Content { get; set; }

        [BindProperty]
        public String? Sign_No { get; set; }

        [BindProperty]
        public Decimal? Hours { get; set; }

        [BindProperty]
        public String? Status { get; set; }

        [BindProperty]
        public List<string> SelectedIds { get; set; }

        [BindProperty]
        public List<int> SelectedItems { get; set; }
        protected SqlDatabase Database { get; set; }    //引用共用函式(SQLService)
        protected Common Common { get; set; }    

        public LeaveModel(SqlDatabase database, Common common)   //注入服務(SQL&log)
        {
            Database = database;
            Common = common;
        }
        public IActionResult OnGet()
        {
            return Page();
        }


        public class LeaveData
        {
            public int? Leave_No { get; set; }
            public String? Leave { get; set; }
            public DateTime? Start_Date { get; set; }
            public DateTime? End_Date { get; set; }
            public String? Leave_Content { get; set; }
            public String? Status { get; set; }
            public String? Sign_No { get; set; }

        }

        private DataTable QueryLeaveData()
        {

            string strSQL = @"
SELECT Leave_Overtime.*, 
               D_Leave.Code_Name AS LeaveName,
               D_Status.Code_Name AS StatusName,
               EMP_Data.EMP_Name AS SignNo
  FROM Leave_Overtime WITH(NOLOCK)
 INNER JOIN DDL_Setting  AS D_Leave WITH(NOLOCK) ON Leave_Overtime.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
 INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON Leave_Overtime.Status = D_Status.Code_No AND D_Status.DDL_No = 'F01' AND D_Status.Code_No = '0'
     LEFT JOIN EMP_Data AS EMP_Data WITH(NOLOCK) ON Leave_Overtime.Sign_No = EMP_Data.EMP_No
WHERE Leave_Overtime.Emp_No=@userName
ORDER BY Leave_No DESC
";
            var userName = User.Identity?.Name;
            SqlCommand command = Database.CreateCommand(strSQL);
            command.Parameters.AddWithValue("@userName", userName);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        public IActionResult OnPostAdd()
        {
            if (Start_Date == null || End_Date == null || string.IsNullOrWhiteSpace(Leave) || string.IsNullOrWhiteSpace(Sign_No) || string.IsNullOrWhiteSpace(Leave_Content))
            {
                TempData["AlertMessage"] = "請填寫所有必填欄位";
                return Page();
            }

            // 檢查日期合法性
            if (End_Date < Start_Date)
            {
                TempData["AlertMessage"] = "結束日期不能大於起始日期";
                return Page();
            }
            DataTable hoursTable = Common.GetLeavehours(Start_Date.ToString(), End_Date.ToString());
            if (hoursTable.Rows.Count > 0)
            {
                int days = hoursTable.Rows[0].Field<int>("Days");
                decimal hours = hoursTable.Rows[0].Field<decimal>("Hours");
                const int HoursPerDay = 8;
                // 將天數換成小時數
                decimal totalHours = days * HoursPerDay + hours;

                DataTable leaveNoTable = Common.GetLeaveNo();
                string leaveNo = leaveNoTable.Rows[0][0].ToString();

                string insertLeaveQuery = @"
                INSERT INTO LEAVE_OVERTIME 
                                         (Leave_No,
                                          Emp_No,
                                          Sign_No,
                                          Start_Date,
                                          End_Date,
                                          Leave_Content,
                                          Status,Leave,
                                          Hours,
                                          Creater,
                                          Create_Date)
                         VALUES (@Leave_No,
                                          @Emp_No,
                                          @Sign_No,
                                          @Start_Date,
                                          @End_Date,
                                          @Leave_Content,
                                          @Status,
                                          @Leave,
                                          @Hours,
                                          @Emp_No,
                                          GetDate())
";

            var Emp_No = User.Identity?.Name;

            SqlCommand command = Database.CreateCommand(insertLeaveQuery);
            command.Parameters.AddWithValue("@Leave_No", leaveNo);
            command.Parameters.AddWithValue("@Emp_No", Emp_No);
            command.Parameters.AddWithValue("@Sign_No", this.Sign_No);
            command.Parameters.AddWithValue("@Start_Date", (Object)Start_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@End_Date", (Object)End_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", '0');
            command.Parameters.AddWithValue("@Leave", this.Leave);
            command.Parameters.AddWithValue("@Leave_Content", Leave_Content);
            command.Parameters.AddWithValue("@Hours", totalHours);
            command.ExecuteNonQuery();

            //call Query
            //dt  存到LIST 顯示View.bag
            ViewData["QueryLeaveData"] = this.QueryLeaveData();
            TempData["AlertMessage"] = "新增成功";
            return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "計算時數失敗";
                return Page();
            }
        }
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)   //每次都會執行
        {
            base.OnPageHandlerExecuting(context);
            ViewData["QueryLeaveData"] = this.QueryLeaveData();

        }
        public IActionResult OnPostSearch()
        {
            try
            {
              
                string searchQuery = @"
SELECT Leave_Overtime.*, 
               D_Leave.Code_Name AS LeaveName,
               D_Status.Code_Name AS StatusName,
               EMP_Data.EMP_Name AS SignNo
  FROM Leave_Overtime WITH(NOLOCK)
 INNER JOIN DDL_Setting  AS D_Leave WITH(NOLOCK) ON Leave_Overtime.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
 INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON Leave_Overtime.Status = D_Status.Code_No AND D_Status.DDL_No = 'F01' AND D_Status.Code_No = '0'
     LEFT JOIN EMP_Data AS EMP_Data WITH(NOLOCK) ON Leave_Overtime.Sign_No = EMP_Data.EMP_No
WHERE 1 = 1
    AND D_Status.Code_No = '0'
    AND (Leave_No = @Leave_No OR ISNULL(@Leave_No,'') ='')
    AND (Leave = @Leave OR ISNULL(@Leave,'') ='')
    AND (Sign_No = @Sign_No OR ISNULL(@Sign_No,'') ='')
    AND (CONVERT(DATE, Start_Date) >= @Start_Date OR ISNULL(@Start_Date, '') = '')
    AND (CONVERT(DATE, End_Date) <= @End_Date OR ISNULL(@End_Date, '') = '')
    AND (LEAVE_CONTENT LIKE '%' + @Leave_Content + '%' OR ISNULL(@Leave_Content, '') = '')
ORDER BY Leave_No DESC
                 ";
                SqlCommand command = Database.CreateCommand(searchQuery);
                command.Parameters.AddWithValue("@Leave_No", (Object)Leave_No! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Leave", (Object)Leave! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Sign_No", (Object)Sign_No! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Start_Date", Start_Date.HasValue ? (object)Start_Date.Value.Date : DBNull.Value);
                command.Parameters.AddWithValue("@End_Date", End_Date.HasValue ? (object)End_Date.Value.Date : DBNull.Value);
                command.Parameters.AddWithValue("@Leave_Content", (Object)Leave_Content! ?? DBNull.Value);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dt);
                ViewData["QueryLeaveData"] = dt;
            }
            catch
            {
                TempData["AlertMessage"] = "查詢時發生錯誤";
            }
            return Page();


        }

        public LeaveData GetLeaveDataByLeaveNo(int leaveNo)
        {

            string strSQL = @"
                    SELECT Leave_No, Leave, Start_Date, End_Date, Leave_Content, Status, Sign_No
                     FROM Leave_Overtime WITH(NOLOCK)
                   WHERE Leave_No = @Leave_No
    ";

            SqlCommand command = Database.CreateCommand(strSQL);
            command.Parameters.AddWithValue("@Leave_No", leaveNo);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new LeaveData
                    {
                        Leave_No = reader.GetInt32(reader.GetOrdinal("Leave_No")),
                        Leave = reader.GetString(reader.GetOrdinal("Leave")),
                        Start_Date = reader.IsDBNull(reader.GetOrdinal("Start_Date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Start_Date")),
                        End_Date = reader.IsDBNull(reader.GetOrdinal("End_Date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("End_Date")),
                        Leave_Content = reader.GetString(reader.GetOrdinal("Leave_Content")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Sign_No = reader.GetString(reader.GetOrdinal("Sign_No")),
                    };
                }
                else
                {
                    // 如果找不到對應的資料，可以根據需求返回 null 或其他適當的值
                    return null;
                }
            }
        }
        public IActionResult OnGetEdit(int leaveNo)
        {
            LeaveData leaveData = GetLeaveDataByLeaveNo(leaveNo);
            if (leaveData != null)
            {
                this.Leave_No = leaveData.Leave_No?.ToString();
                this.Leave = leaveData.Leave;
                this.Start_Date = leaveData.Start_Date;
                this.End_Date = leaveData.End_Date;
                this.Leave_Content = leaveData.Leave_Content;
                this.Sign_No = leaveData.Sign_No;
            }

            return Page();
        }
        public IActionResult OnPostUpdated()
        {

            if (string.IsNullOrEmpty(Leave_No))
            {
                TempData["AlertMessage"] = "請選擇要更新的資料";
                return RedirectToPage();
            }
            Console.WriteLine($"Before recalculation - Start_Date: {Start_Date}, End_Date: {End_Date}");

            // 重新計算 totalHours
            DataTable hoursTable = Common.GetLeavehours(Start_Date.ToString(), End_Date.ToString());
            if (hoursTable.Rows.Count > 0)
            {
                int days = hoursTable.Rows[0].Field<int>("Days");
                decimal hours = hoursTable.Rows[0].Field<decimal>("Hours");
                const int HoursPerDay = 8;
                // 將天數換成小時數
                decimal totalHours = days * HoursPerDay + hours;

                // 添加日誌語句
                Console.WriteLine($"After recalculation - totalHours: {totalHours}");


                string updatedLeaveQuery = @"
                UPDATE Leave_Overtime
                        SET Leave = @Leave,
                               Hours=@Hours,
                               Start_Date = @Start_Date,
                               End_Date = @End_Date,
                               Leave_Content = @Leave_Content,
                               Sign_No = @Sign_No,
                               Modifier = @Emp_No,
                               Modify_Date = GetDate()
                WHERE Leave_No = @Leave_No;
                    ";
                var Emp_No = User.Identity?.Name;
                SqlCommand command = Database.CreateCommand(updatedLeaveQuery);
                command.Parameters.AddWithValue("@Emp_No", Emp_No);
                command.Parameters.AddWithValue("@Leave_No", Leave_No);
                command.Parameters.AddWithValue("@Leave", Leave);
                command.Parameters.AddWithValue("@Hours", totalHours);
                command.Parameters.AddWithValue("@Start_Date", (Object)Start_Date! ?? DBNull.Value);
                command.Parameters.AddWithValue("@End_Date", (Object)End_Date! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Leave_Content", Leave_Content);
                command.Parameters.AddWithValue("@Sign_No", Sign_No);

                command.ExecuteNonQuery();

                // Call Query
                // dt  存到 LIST 顯示 View.bag
                ViewData["QueryLeaveData"] = this.QueryLeaveData();
                TempData["AlertMessage"] = "更新完成";
                return RedirectToPage();
            }else{
                TempData["AlertMessage"] = "計算時數失敗";
                return Page();
            }

        }

        public IActionResult OnPostDelete(List<int> SelectedItems)
        {

            if (SelectedItems != null && SelectedItems.Any())
            {
                // 將選中的項目進行刪除操作
                string deleteQuery = @"
                            DELETE 
                              FROM Leave_Overtime 
                            WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(deleteQuery);
                command.ExecuteNonQuery();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇要刪除資料";
                return RedirectToPage();
            }

            // 刪除完成後，重新加載資料並顯示在 View 中
            ViewData["QueryLeaveData"] = QueryLeaveData();
            TempData["AlertMessage"] = "刪除完成";
            return RedirectToPage();
        }
        public IActionResult OnPostSend(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                string updatedQuery = @"
                         UPDATE Leave_Overtime
                                  SET Status = '1',
                                          Submit = @Emp_No,
                                          Submit_Date = GetDate()
                           WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                var Emp_No = User.Identity?.Name;

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.Parameters.AddWithValue("@Emp_No", Emp_No);
                command.ExecuteNonQuery();
                ViewData["QueryLeaveData"] = this.QueryLeaveData();
                TempData["AlertMessage"] = "提交成功";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇要提交的資料";
                return RedirectToPage();
            }

        }

        public class CalculateTimeRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        public JsonResult OnPostCalculateTime([FromBody] CalculateTimeRequest requestModel)
        {

            DateTime startDate = requestModel.StartDate;
            DateTime endDate = requestModel.EndDate;
            if (startDate == null || endDate == null)
            {
                return new JsonResult(new { success = false, message = "請填寫起始日期與結束日期" });
            }

            // 檢查日期合法性
            if (endDate < startDate)
            {
                return new JsonResult(new { success = false, message = "結束日期不能大於起始日期" });
            }

            // 計算請假時數
            DataTable dt = Common.GetLeavehours(startDate.ToString(), endDate.ToString());
            int days = 0;
            decimal hours = 0;
            if (dt.Rows.Count > 0)
            {
                days = dt.Rows[0].Field<int>("Days");
                hours = dt.Rows[0].Field<decimal>("Hours");
            }
            else
            {
                return new JsonResult(new { success = false, message = "計算時數失敗" });
            }


            return new JsonResult(new { success = true, days = days, hours = hours });

        }
    }
}
