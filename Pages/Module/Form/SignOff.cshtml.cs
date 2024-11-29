using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;



//using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using HR_WorkFlow.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.NetworkInformation;



namespace HR_WorkFlow.Pages.Module.Form
{
    [Authorize]
    public class SignOffModel : PageModel
    {        
        [BindProperty]
        public string Status { get; set; }

        [BindProperty]
        public string Leave_No { get; set; }

        [BindProperty]
        public DateTime? Create_Date { get; set; }

        [BindProperty]
        public string ContentMessage { get; set; }

        protected SqlDatabase Database { get; set; }    //引用共用函式(SQLService)


        public SignOffModel(SqlDatabase database)   //注入服務(SQL&log) 
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
              D_Status.Code_Name AS StatusName,
              E.Emp_Name,
              L.Leave_No,
              D_Leave.Code_Name AS LeaveName,
              L.Start_Date,
              L.End_Date,
              L.Leave_Content
  FROM Leave_Overtime L WITH(NOLOCK)
     LEFT JOIN EMP_Data E WITH(NOLOCK) ON L.Emp_No= E.EMP_No
 INNER JOIN DDL_Setting AS D_Leave WITH(NOLOCK) ON L.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
 INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON L.status = D_Status.Code_No AND D_Status.DDL_No = 'F01' AND D_Status.Trade_Code LIKE '%SignOff%'
WHERE L.Sign_No = @EmpNo
ORDER BY L.Leave_No DESC
               ";


            SqlCommand command = Database.CreateCommand(strSQL);
            var userName = User.Identity?.Name;
            command.Parameters.AddWithValue("@EmpNo", userName);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }

        //查詢==========================================
        public IActionResult OnPostQuery() {
            //SQL 沒排整齊 只需查簽核人跟代理人的登入的員編
            try
            {
                string query = @"
SELECT L.Create_Date,
               D_Status.Code_Name AS StatusName,
               E.Emp_Name,
               L.Leave_No,
               D_Leave.Code_Name AS LeaveName,
               L.Start_Date,
               L.End_Date,
               L.Leave_Content
  FROM Leave_Overtime L WITH(NOLOCK)
     LEFT JOIN EMP_Data E WITH(NOLOCK) ON L.Emp_No= E.EMP_No
 INNER JOIN DDL_Setting AS D_Leave WITH(NOLOCK) ON L.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
 INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON L.Status = D_Status.Code_No AND D_Status.DDL_No = 'F01' 
WHERE 1 = 1
     AND L.Sign_No = @EmpNo
     AND (L.Status = @Status OR ISNULL(@Status,'') ='')
     AND (L.Leave_No = @Leave_No OR ISNULL(@Leave_No,'') ='')
     AND (CONVERT(DATE, L.Create_Date) = @Create_Date OR ISNULL(@Create_Date,'') = '')
ORDER BY L.Leave_No DESC
                 ";

                SqlCommand command = Database.CreateCommand(query);
                var userName = User.Identity?.Name;
                command.Parameters.AddWithValue("@EmpNo", userName);
                command.Parameters.AddWithValue("@Status", (Object)Status! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Leave_No", (Object)Leave_No! ?? DBNull.Value);
                string? formattedDate = Create_Date?.ToString("yyyy-MM-dd");
                command.Parameters.AddWithValue("@Create_Date", formattedDate ?? "");

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dt);

                TempData["Status"] = Status;
                TempData["Leave_No"] = Leave_No;
                TempData["Create_Date"] = Create_Date;

                ViewData["QueryResult"] = dt;

            }
            catch 
            {
                TempData["AlertMessage"] = "查詢時發生錯誤";
            }
            return Page();
          }

        //審核動作======================================
        public IActionResult OnPostSubmit(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                // 檢查是否有已審核或駁回的資料
                //if (HasApprovedOrRejectedStatus(SelectedItems))
                //{
                //    TempData["AlertMessage"] = "已審核或駁回的資料無法再更改狀態。";
                //    return RedirectToPage();
                //}
                if (HasStatus11(SelectedItems))
                {
                    TempData["AlertMessage"] = "審核中無法撤銷";
                    return RedirectToPage();
                }

                // 將選中的項目狀態改為通過
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '2',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "審核已通過";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇資料";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostReject(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus11(SelectedItems))
                {
                    TempData["AlertMessage"] = "審核中無法撤銷";
                    return RedirectToPage();
                }

                // 將選中的項目狀態改為駁回
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '3',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "審核已駁回";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇資料";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostCancelApproved(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus1(SelectedItems))
                {
                    TempData["AlertMessage"] = "撤銷中無法審核";
                    return RedirectToPage();
                }

                // 將選中的項目狀態改為撤銷通過
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '12',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "撤銷已通過";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇資料";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostCancelRejected(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus1(SelectedItems))
                {
                    TempData["AlertMessage"] = "撤銷中無法審核";
                    return RedirectToPage();
                }

                // 將選中的項目狀態改為撤銷駁回
                string updatedQuery = @"
UPDATE Leave_Overtime
        SET Status = '13',
                Sign_Date = GetDate()
 WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "撤銷已駁回";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "請選擇資料";
                return RedirectToPage();
            }
        }

        //public IActionResult OnPostLeaveContent(string leaveContent)
        //{
        //    TempData["AlertMessage"] = leaveContent;
        //    return RedirectToPage();
        //}

        //        private bool HasApprovedOrRejectedStatus(List<int> selectedItems)
        //        {
        //            string query = @"
        //SELECT COUNT(*)
        //  FROM Leave_Overtime
        //WHERE Leave_No IN (" + string.Join(",", selectedItems) + @")
        //     AND Status IN ('2', '3','12','13')";

        //            SqlCommand command = Database.CreateCommand(query);
        //            int count = (int)command.ExecuteScalar();

        //            // 如果有已審核或駁回的資料，返回 true；否則，返回 false。
        //            return count > 0;
        //        }

        private bool HasStatus11(List<int> SelectedItems)
        {
            string checkQuery = @"
SELECT COUNT(*) 
  FROM Leave_Overtime
WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ") AND Status = '11'";

            SqlCommand command = Database.CreateCommand(checkQuery);
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }

        private bool HasStatus1(List<int> SelectedItems)
        {
            string checkQuery = @"
SELECT COUNT(*) 
  FROM Leave_Overtime WITH(NOLOCK)
WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ") AND Status = '1'";

            SqlCommand command = Database.CreateCommand(checkQuery);
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }
    }
}
