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

        protected SqlDatabase Database { get; set; }    //�ޥΦ@�Ψ禡(SQLService)


        public SignOffModel(SqlDatabase database)   //�`�J�A��(SQL&log) 
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

        //�d��==========================================
        public IActionResult OnPostQuery() {
            //SQL �S�ƾ�� �u�ݬdñ�֤H��N�z�H���n�J�����s
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
                TempData["AlertMessage"] = "�d�߮ɵo�Ϳ��~";
            }
            return Page();
          }

        //�f�ְʧ@======================================
        public IActionResult OnPostSubmit(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                // �ˬd�O�_���w�f�֩λ�^�����
                //if (HasApprovedOrRejectedStatus(SelectedItems))
                //{
                //    TempData["AlertMessage"] = "�w�f�֩λ�^����ƵL�k�A��窱�A�C";
                //    return RedirectToPage();
                //}
                if (HasStatus11(SelectedItems))
                {
                    TempData["AlertMessage"] = "�f�֤��L�k�M�P";
                    return RedirectToPage();
                }

                // �N�襤�����ت��A�אּ�q�L
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '2',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "�f�֤w�q�L";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "�п�ܸ��";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostReject(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus11(SelectedItems))
                {
                    TempData["AlertMessage"] = "�f�֤��L�k�M�P";
                    return RedirectToPage();
                }

                // �N�襤�����ت��A�אּ��^
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '3',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "�f�֤w��^";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "�п�ܸ��";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostCancelApproved(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus1(SelectedItems))
                {
                    TempData["AlertMessage"] = "�M�P���L�k�f��";
                    return RedirectToPage();
                }

                // �N�襤�����ت��A�אּ�M�P�q�L
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '12',
                 Sign_Date = GetDate()
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "�M�P�w�q�L";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "�п�ܸ��";
                return RedirectToPage();
            }
        }

        public IActionResult OnPostCancelRejected(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                if (HasStatus1(SelectedItems))
                {
                    TempData["AlertMessage"] = "�M�P���L�k�f��";
                    return RedirectToPage();
                }

                // �N�襤�����ت��A�אּ�M�P��^
                string updatedQuery = @"
UPDATE Leave_Overtime
        SET Status = '13',
                Sign_Date = GetDate()
 WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
                ViewData["QueryResult"] = QueryResult();
                TempData["AlertMessage"] = "�M�P�w��^";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "�п�ܸ��";
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

        //            // �p�G���w�f�֩λ�^����ơA��^ true�F�_�h�A��^ false�C
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
