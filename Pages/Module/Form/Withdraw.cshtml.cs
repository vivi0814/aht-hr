using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using static HR_WorkFlow.Pages.Module.Form.WithdrawModel;

namespace HR_WorkFlow.Pages.Module.Form
{
    [Authorize]
    public class WithdrawModel : PageModel
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
        protected SqlDatabase Database { get; set; }    //�ޥΦ@�Ψ禡(SQLService)
        public WithdrawModel(SqlDatabase database)
        {
            Database = database;
        }
        public IActionResult OnGet()
        {
            return Page();
        }

        public class WithdrawData
        {
            public int? Leave_No { get; set; }
            public String? Leave { get; set; }
            public DateTime? Start_Date { get; set; }
            public DateTime? End_Date { get; set; }
            public String? Leave_Content { get; set; }
            public String? Status { get; set; }
            public String? Sign_No { get; set; }

        }

        private DataTable QueryWithdrawData()
        {
            string strSQL = @"
SELECT Leave_Overtime.*, 
               D_Leave.Code_Name AS LeaveName,
               D_Status.Code_Name AS StatusName
  FROM Leave_Overtime WITH(NOLOCK)
  INNER JOIN DDL_Setting AS D_Leave WITH(NOLOCK) ON Leave_Overtime.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
  INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON Leave_Overtime.Status = D_Status.Code_No AND D_Status.DDL_No = 'F01' AND D_Status.Trade_Code LIKE '%Withdraw%'
WHERE Leave_Overtime.EMP_No = @EmpNo
 ORDER BY Leave_No DESC
             ";

            var userName = User.Identity?.Name;
            SqlCommand command = Database.CreateCommand(strSQL);
            command.Parameters.AddWithValue("@EmpNo", userName);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }

        //�N���A���令�M�P������A�ƥѤ�ñ�֤H�Y�����]�|��s
        public IActionResult OnPostAdd()
        {
            if (string.IsNullOrWhiteSpace(Sign_No) || string.IsNullOrWhiteSpace(Leave_Content))
            {
                TempData["AlertMessage"] = "�ж�g�Ҧ��������";
                return Page();
            }

            string insertWithdrawQuery = @"
UPDATE Leave_Overtime
         SET Status = '10',
                 Leave_Content = @Leave_Content,
                 Sign_No = @Sign_No,
                 CancelCreate_Date = GetDate()
 WHERE  Leave_No = @Leave_No";

            SqlCommand command = Database.CreateCommand(insertWithdrawQuery);
            command.Parameters.AddWithValue("@Leave_No", Leave_No);
            //command.Parameters.AddWithValue("@Leave", Leave);
            //command.Parameters.AddWithValue("@Start_Date", (Object)Start_Date! ?? DBNull.Value);
            //command.Parameters.AddWithValue("@End_Date", (Object)End_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Leave_Content", Leave_Content);
            command.Parameters.AddWithValue("@Sign_No", Sign_No);
            command.ExecuteNonQuery();

            ViewData["QueryWithdrawData"] = this.QueryWithdrawData();
            TempData["AlertMessage"] = "�w�ӽкM�P";
            return RedirectToPage();

        }
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)   //�C�����|����
        {
            base.OnPageHandlerExecuting(context);
            ViewData["QueryWithdrawData"] = this.QueryWithdrawData();

        }

        public IActionResult OnPostSearch()
        {
            try
            {
                //if (!(string.IsNullOrEmpty(Start_Date.ToString()) == false || string.IsNullOrEmpty(End_Date.ToString()) == false))
                //{
                //    TempData["AlertMessage"] = "�п�J�ɶ�";
                //    return Page();
                //}
                string searchQuery = @"
SELECT L.*, 
               D_Leave.Code_Name AS LeaveName,
               D_Status.Code_Name AS StatusName
  FROM Leave_Overtime L WITH(NOLOCK)
     LEFT JOIN EMP_Data E WITH(NOLOCK) ON L.Emp_No= E.EMP_No
 INNER JOIN DDL_Setting AS D_Leave WITH(NOLOCK) ON L.Leave = D_Leave.Code_No AND D_Leave.DDL_No = 'L01'
 INNER JOIN DDL_Setting AS D_Status WITH(NOLOCK) ON L.Status = D_Status.Code_No AND D_Status.DDL_No = 'F01' AND D_Status.Trade_Code LIKE '%Withdraw%'
WHERE 1 = 1
     AND L.Emp_No = @EmpNo
     AND (L.Status = @Status OR ISNULL(@Status,'') ='')
     AND (L.Sign_No = @Sign_No OR ISNULL(@Sign_No,'') ='')
     AND (L.LEAVE_CONTENT LIKE '%' + @Leave_Content + '%' OR ISNULL(@Leave_Content, '') = '')
ORDER BY Leave_No DESC
                 ";
                SqlCommand command = Database.CreateCommand(searchQuery);
                var userName = User.Identity?.Name;
                command.Parameters.AddWithValue("@EmpNo", userName);
                command.Parameters.AddWithValue("@Status", (Object)Status! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Sign_No", (Object)Sign_No! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Leave_Content", (Object)Leave_Content! ?? DBNull.Value);



                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dt);
                ViewData["QueryWithdrawData"] = dt;
            }
            catch
            {
                ViewData["AlertMessage"] = "�d�߮ɵo�Ϳ��~";
            }
            return Page();


        }

        public WithdrawData GetWithdrawDataByLeaveNo(int leaveNo)
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
                    return new WithdrawData
                    {
                        Leave_No = reader.GetInt32(reader.GetOrdinal("Leave_No")),
                        Leave = reader.GetString(reader.GetOrdinal("Leave")),
                        Start_Date = reader.IsDBNull(reader.GetOrdinal("Start_Date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Start_Date")),
                        End_Date = reader.IsDBNull(reader.GetOrdinal("End_Date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("End_Date")),
                        Leave_Content = reader.GetString(reader.GetOrdinal("Leave_Content")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Sign_No = reader.GetString(reader.GetOrdinal("Sign_No")),
                        // ��L�ݩ�...
                    };
                }
                else
                {
                    // �p�G�䤣���������ơA�i�H�ھڻݨD��^ null �Ψ�L�A����
                    return null;
                }
            }
        }
        public IActionResult OnGetEdit(int leaveNo)
        {
            WithdrawData withdrawData = GetWithdrawDataByLeaveNo(leaveNo);
            if (withdrawData != null)
            {
                this.Leave_No = withdrawData.Leave_No?.ToString();
                this.Status = withdrawData.Status?.ToString();
                this.Leave = withdrawData.Leave;
                this.Start_Date = withdrawData.Start_Date;
                this.End_Date = withdrawData.End_Date;
                this.Leave_Content = withdrawData.Leave_Content;
                this.Sign_No = withdrawData.Sign_No;
            }

            return Page();
        }
        public IActionResult OnPostUpdated()
        {

            if (string.IsNullOrEmpty(Leave_No))
            {
                TempData["AlertMessage"] = "�п�ܭn��s�����";
                return RedirectToPage();
            }
            if (Status == "2")
            {
                TempData["AlertMessage"] = "�w�f�ֵL�k��s�M�P�����檺���";
                return Page();
            }
            string updatedWithdrawQuery = @"
UPDATE Leave_Overtime
         SET Leave_Content = @Leave_Content,
                 Sign_No = @Sign_No,
                 CancelModify_Date = GetDate()
  WHERE Leave_No = @Leave_No;
             ";

            
            SqlCommand command = Database.CreateCommand(updatedWithdrawQuery);
            command.Parameters.AddWithValue("@Leave_No", Leave_No);
            command.Parameters.AddWithValue("@Leave_Content", Leave_Content);
            command.Parameters.AddWithValue("@Sign_No", Sign_No);

            command.ExecuteNonQuery();

            // Call Query
            // dt  �s�� LIST ��� View.bag
            ViewData["QueryWithdrawData"] = this.QueryWithdrawData();
            TempData["AlertMessage"] = "��s����";
            return RedirectToPage();


        }

        //�N�M�P�����檺���A��^�w�f��
        public IActionResult OnPostDelete(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                foreach (int leaveNo in SelectedItems)
                {
                    string getStatusQuery = @"
SELECT Status 
  FROM Leave_Overtime WITH(NOLOCK)
WHERE Leave_No = @LeaveNo
                     ";
                    SqlCommand getStatusCommand = Database.CreateCommand(getStatusQuery);
                    getStatusCommand.Parameters.AddWithValue("@LeaveNo", leaveNo);
                    object statusObj = getStatusCommand.ExecuteScalar();

                    if (statusObj != null && statusObj != DBNull.Value)
                    {
                        string status = statusObj.ToString();

                        if (status == "2")
                        {
                            TempData["AlertMessage"] = "�w�f�ֵL�k�R���M�P�����檺���";
                            return Page();
                        }
                    }
                }
                // �Ҧ���ƪ� Status �����\�R���A����R���ާ@
                string deleteQuery =@"
UPDATE Leave_Overtime 
         SET Status = '2', 
                 CancelModify_Date = GetDate() 
  WHERE Leave_No IN(" + string.Join(",", SelectedItems) + ")";
                SqlCommand deleteCommand = Database.CreateCommand(deleteQuery);
                deleteCommand.ExecuteNonQuery();

                ViewData["QueryWithdrawData"] = this.QueryWithdrawData();
                TempData["AlertMessage"] = "�w�R���M�P������";
                return RedirectToPage();
            }
            else
            {
                TempData["AlertMessage"] = "�п�ܭn�R�����";
                return RedirectToPage();
            }

        }
        public IActionResult OnPostSend(List<int> SelectedItems)
        {
            if (SelectedItems != null && SelectedItems.Any())
            {
                // �ˬd�O�_���ݴ��檺��ƪ� Status �� '2'
                string checkQuery = @"
SELECT COUNT(*) 
  FROM Leave_Overtime WITH(NOLOCK)
WHERE Leave_No IN (" + string.Join(",", SelectedItems) + @")
     AND Status = '2'";

                SqlCommand checkCommand = Database.CreateCommand(checkQuery);
                int status2Count = (int)checkCommand.ExecuteScalar();

                if (status2Count > 0)
                {
                    // �p�G�� Status �� '2' ����ơA�h��^����ܿ��~�T��
                    TempData["AlertMessage"] = "�s�b�w�f�֪���ơA�L�k����";
                    return RedirectToPage();
                }

                // �p�G�S�� Status �� '2' ����ơA�����s�ާ@
                string updatedQuery = @"
UPDATE Leave_Overtime
         SET Status = '11',
                 CancelSubmit_Date = GetDate() 
  WHERE Leave_No IN (" + string.Join(",", SelectedItems) + ")";

                SqlCommand command = Database.CreateCommand(updatedQuery);
                command.ExecuteNonQuery();
            }

            ViewData["QueryWithdrawData"] = this.QueryWithdrawData();
            TempData["AlertMessage"] = "���榨�\";
            return RedirectToPage();
        }
    }
}
