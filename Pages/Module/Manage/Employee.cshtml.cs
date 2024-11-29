using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using static HR_WorkFlow.Pages.Module.Manage.EmployeeModel;
using DataTable = System.Data.DataTable;

namespace HR_WorkFlow.Pages.Module.Manage
{
    [Authorize]
    public class EmployeeModel : PageModel
    {
        [BindProperty]
        public String? EMP_No { get; set; }

        [BindProperty]
        public String? EMP_No1 { get; set; }
        [BindProperty]
        public String? EMP_Name { get; set; }
        [BindProperty]
        public String? Role { get; set; }
        [BindProperty]
        public String? Department { get; set; }
        [BindProperty]
        public String? Position { get; set; }
        [BindProperty]
        public String? Agent { get; set; }
        [BindProperty]
        public DateTime? Arrival_Date { get; set; }
        [BindProperty]
        public int? jobTenure { get; set; }
        [BindProperty]
        public DateTime? Actual_Arrival_Date { get; set; }
        [BindProperty]
        public String? Sign_No { get; set; }
        [BindProperty]
        public String? Status { get; set; }
        [BindProperty]
        public DateTime? Resignation_Date { get; set; }
        [BindProperty]
        public String? Gender { get; set; }
        [BindProperty]
        public DateTime? Date_Of_Birth { get; set; }
        [BindProperty]
        public String? ID { get; set; }
        [BindProperty]
        public String? Cellphone { get; set; }
        [BindProperty]
        public String? Telephone { get; set; }
        [BindProperty]
        public String? Residence_address { get; set; }
        [BindProperty]
        public String? Address { get; set; }
        [BindProperty]
        public String? Email { get; set; }
        [BindProperty]
        public String? Emergency_Contact { get; set; }
        [BindProperty]
        public String? Emergency_Contact_Phone { get; set; }
        protected SqlDatabase Database { get; set; }    //引用共用函式(SQLService)

        public EmployeeModel(SqlDatabase database)
        {
            Database = database;
        }

        public class EmployeeData
        {
            public String? EMP_No { get; set; }
            public String? EMP_Name { get; set; }
            public String? Role { get; set; }
            public String? Department { get; set; }
            public String? Position { get; set; }
            public String? Agent { get; set; }
            public DateTime? Arrival_Date { get; set; }
            public int? Job_Tenure { get; set; }
            public DateTime? Actual_Arrival_Date { get; set; }
            public String? Sign_No { get; set; }
            public String? Status { get; set; }
            public DateTime? Resignation_Date { get; set; }
            public String? Gender { get; set; }
            public DateTime? Date_Of_Birth { get; set; }
            public String? ID { get; set; }
            public String? Cellphone { get; set; }
            public String? Telephone { get; set; }
            public String? Residence_address { get; set; }
            public String? Address { get; set; }
            public String? Email { get; set; }
            public String? Emergency_Contact { get; set; }
            public String? Emergency_Contact_Phone { get; set; }

        }
        
        private DataTable QueryEmployeeData()
        {
            string strSQL = @"
	    SELECT E.EMP_No,
				        E.EMP_Name,
		  		        D_Dept.Code_Name AS Dept,
				        D_Position.Code_Name AS PositionName,
				        D_Role.Code_Name AS Rol,
				        E_Sign.EMP_Name AS SignNo,
				        E_Agent.EMP_Name AS S_Agent
           FROM EMP_Data E WITH (NOLOCK)
    LEFT JOIN DDL_Setting AS D_Role WITH (NOLOCK) ON E.Role = D_Role.Code_No AND D_Role.DDL_No = 'C01' 
    LEFT JOIN DDL_Setting AS D_Dept WITH (NOLOCK) ON E.Department = D_Dept.Code_No AND D_Dept.DDL_No = 'D01' 
    LEFT JOIN DDL_Setting AS D_Position WITH (NOLOCK) ON E.Position = D_Position.Code_No AND D_Position.DDL_No = 'T01' 
INNER JOIN EMP_Data AS E_Sign WITH (NOLOCK) ON E.Sign_No = E_Sign.EMP_No
INNER JOIN EMP_Data AS E_Agent WITH (NOLOCK) ON E.Agent = E_Agent.EMP_No
WHERE E.Status=1
   ORDER BY E.EMP_No DESC
                     ";

            SqlCommand command = Database.CreateCommand(strSQL);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)   //每次都會執行
        {
            base.OnPageHandlerExecuting(context);
            ViewData["QueryEmployeeData"] = this.QueryEmployeeData();

        }
        public IActionResult OnPostAdd()
        {
           
            if (string.IsNullOrWhiteSpace(EMP_No) || string.IsNullOrWhiteSpace(EMP_Name) || string.IsNullOrWhiteSpace(Role) ||
                string.IsNullOrWhiteSpace(Department) || string.IsNullOrWhiteSpace(Position) || string.IsNullOrWhiteSpace(Agent) ||
                Arrival_Date == null || Actual_Arrival_Date == null || string.IsNullOrWhiteSpace(Sign_No) ||
                string.IsNullOrWhiteSpace(Gender) || Date_Of_Birth == null || string.IsNullOrWhiteSpace(ID) ||
                string.IsNullOrWhiteSpace(Cellphone) || string.IsNullOrWhiteSpace(Telephone) ||
                string.IsNullOrWhiteSpace(Residence_address) || string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Emergency_Contact) || string.IsNullOrWhiteSpace(Emergency_Contact_Phone))
            {
                TempData["AlertMessage"] = "請填寫必填欄位";
                return Page();
            }
            if (IsEmpNoDuplicate(EMP_No))
            {
                TempData["AlertMessage"] = "員編已經存在，請使用其他值";
                return Page();
            }
            var jobTenureResult = OnPostCalculateJobTenure(new CalculateJobTenure { Actual_Arrival_Date = Actual_Arrival_Date ?? DateTime.MinValue });

            if (jobTenureResult.Value is JsonResult jsonResult && jsonResult.Value is JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty("jobTenure", out var jobTenureProperty))
                {
                    jobTenure = jobTenureProperty.GetInt32();
                }
            }


            string insertEmployeeQuery = @"
INSERT INTO EMP_Data
                        (EMP_No ,
                         EMP_Name,
                         Pass_Word,
                         Role,
                         Department, 
                         Position,
                         Agent,
                         Arrival_Date, 
                         Job_Tenure,
                         Actual_Arrival_Date,
                         Sign_No,
                         Status,
                         Resignation_Date,
                         Gender, 
                         Date_Of_Birth,
                         ID, 
                         Cellphone,
                         Telephone,
                         Residence_address,
                         Address,
                         Email,
                         Emergency_Contact,
                         Emergency_Contact_Phone,
                         Creater,
                         Create_Date,
                         Login_Time)
         VALUES (@EMP_No ,
                          @EMP_Name,
                          @EMP_No,
                          @Role,
                          @Department,
                          @Position,
                          @Agent,
                          @Arrival_Date,
                          @Job_Tenure,
                          @Actual_Arrival_Date,
                          @Sign_No,
                          @Status,
                          @Resignation_Date,
                          @Gender,
                          @Date_Of_Birth,
                          @ID,
                          @Cellphone,
                          @Telephone,
                          @Residence_address,
                          @Address,
                          @Email,
                          @Emergency_Contact,
                          @Emergency_Contact_Phone,
                          @Creater,
                          GetDate(),
                          GetDate()
                            )
";

            var Creater = User.Identity?.Name;

            SqlCommand command = Database.CreateCommand(insertEmployeeQuery);
            command.Parameters.AddWithValue("@EMP_No", EMP_No);
            command.Parameters.AddWithValue("@EMP_Name", EMP_Name);
            command.Parameters.AddWithValue("@Role", Role);
            command.Parameters.AddWithValue("@Department", Department);
            command.Parameters.AddWithValue("@Position", Position);
            command.Parameters.AddWithValue("@Agent", Agent);
            command.Parameters.AddWithValue("@Job_Tenure", jobTenure);
            command.Parameters.AddWithValue("@Sign_No", Sign_No);
            command.Parameters.AddWithValue("@Arrival_Date", (Object)Arrival_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Actual_Arrival_Date", (Object)Actual_Arrival_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Resignation_Date", (Object)Resignation_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Date_Of_Birth", (Object)Date_Of_Birth! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", Resignation_Date.HasValue && Resignation_Date.Value.Date <= DateTime.Now.Date ? 2 : 1);
            command.Parameters.AddWithValue("@Gender", value: Gender);
            command.Parameters.AddWithValue("@ID", ID);
            command.Parameters.AddWithValue("@Cellphone", Cellphone);
            command.Parameters.AddWithValue("@Telephone", Telephone);
            command.Parameters.AddWithValue("@Residence_address", Residence_address);
            command.Parameters.AddWithValue("@Address", Address);
            command.Parameters.AddWithValue("@Email", Email);
            command.Parameters.AddWithValue("@Emergency_Contact",Emergency_Contact);
            command.Parameters.AddWithValue("@Emergency_Contact_Phone",Emergency_Contact_Phone);
            command.Parameters.AddWithValue("@Creater", Creater);
            command.ExecuteNonQuery();

            //call Query
            //dt  存到LIST 顯示View.bag
            ViewData["QueryEmployeeData"] = this.QueryEmployeeData();
            TempData["AlertMessage"] = "新增成功";
            return RedirectToPage();
        }
        private bool IsEmpNoDuplicate(string empNo)
        {
            string checkDuplicateQuery = "SELECT COUNT(*) FROM EMP_Data WHERE EMP_No = @EmpNo";

            using (SqlCommand command = Database.CreateCommand(checkDuplicateQuery))
            {
                command.Parameters.AddWithValue("@EmpNo", empNo);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
        public IActionResult OnPostSearch()
        {
            try
            {
                string searchQuery = @"
SELECT E.*,
		  	   D_Dept.Code_Name AS Dept,
		       D_Position.Code_Name AS PositionName,
			   D_Role.Code_Name AS Rol,
			   E_Sign.EMP_Name AS SignNo,
			   E_Agent.EMP_Name AS S_Agent
  FROM EMP_Data E WITH (NOLOCK)
     LEFT JOIN DDL_Setting AS D_Role WITH (NOLOCK) ON E.Role = D_Role.Code_No AND D_Role.DDL_No = 'C01' 
     LEFT JOIN DDL_Setting AS D_Dept WITH (NOLOCK) ON E.Department = D_Dept.Code_No AND D_Dept.DDL_No = 'D01' 
     LEFT JOIN DDL_Setting AS D_Position WITH (NOLOCK) ON E.Position = D_Position.Code_No AND D_Position.DDL_No = 'T01' 
 INNER JOIN EMP_Data AS E_Sign WITH (NOLOCK) ON E.Sign_No = E_Sign.EMP_No
 INNER JOIN EMP_Data AS E_Agent WITH (NOLOCK) ON E.Agent = E_Agent.EMP_No
WHERE 1 = 1
    AND (E.EMP_No = @EMP_No OR ISNULL(@EMP_No,'') ='')
    AND (E.EMP_Name LIKE '%' + @EMP_Name + '%' OR ISNULL(@EMP_Name, '') = '')
    AND (E.Role = @Role OR ISNULL(@Role,'') ='')
    AND (E.Position = @Position OR ISNULL(@Position,'') ='')
    AND (E.Agent = @Agent OR ISNULL(@Agent,'') ='')
    AND (E.Department = @Department OR ISNULL(@Department,'') ='')
    AND (E.Arrival_Date = @Arrival_Date OR ISNULL(@Arrival_Date,'') ='')
    AND (E.Actual_Arrival_Date = @Actual_Arrival_Date OR ISNULL(@Actual_Arrival_Date,'') ='')
    AND (E.Sign_No = @Sign_No OR ISNULL(@Sign_No,'') ='')
    AND (E.Status = @Status OR ISNULL(@Status,'') ='')
    AND (E.Resignation_Date = @Resignation_Date OR ISNULL(@Resignation_Date,'') ='')
    AND (E.Gender = @Gender OR ISNULL(@Gender,'') ='')
    AND (E.Date_Of_Birth = @Date_Of_Birth OR ISNULL(@Date_Of_Birth,'') ='')
    AND (E.ID = @ID OR ISNULL(@ID,'') ='')
    AND (E.Cellphone = @Cellphone OR ISNULL(@Cellphone,'') ='')
    AND (E.Telephone = @Telephone OR ISNULL(@Telephone,'') ='')
    AND (E.Residence_address LIKE '%' + @Residence_address + '%' OR ISNULL(@Residence_address, '') = '')
    AND (E.Address LIKE '%' + @Address + '%' OR ISNULL(@Address, '') = '')
    AND (E.Email LIKE '%' + @Email + '%' OR ISNULL(@Email, '') = '')
    AND (E.Emergency_Contact = @Emergency_Contact OR ISNULL(@Emergency_Contact,'') ='')
    AND (E.Emergency_Contact_Phone LIKE '%' + @Emergency_Contact_Phone + '%' OR ISNULL(@Emergency_Contact_Phone, '') = '')
ORDER BY EMP_No DESC
                 ";
            SqlCommand command = Database.CreateCommand(searchQuery);
            command.Parameters.AddWithValue("@EMP_No", (Object)EMP_No! ?? DBNull.Value);
            command.Parameters.AddWithValue("@EMP_Name", (Object)EMP_Name! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Role", (Object)Role! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Department", (Object)Department! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Position", (Object)Position! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Agent", (Object)Agent! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Sign_No", (Object)Sign_No! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Arrival_Date", (Object)Arrival_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Actual_Arrival_Date", (Object)Actual_Arrival_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Resignation_Date", (Object)Resignation_Date! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Date_Of_Birth", (Object)Date_Of_Birth! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", (Object)Status! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Gender", (Object)Gender! ?? DBNull.Value);
            command.Parameters.AddWithValue("@ID", (Object)ID! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Cellphone", (Object)Cellphone! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telephone", (Object)Telephone! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Residence_address", (Object)Residence_address! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (Object)Address! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (Object)Email! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Emergency_Contact", (Object)Emergency_Contact! ?? DBNull.Value);
            command.Parameters.AddWithValue("@Emergency_Contact_Phone", (Object)Emergency_Contact_Phone! ?? DBNull.Value);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            ViewData["QueryEmployeeData"] = dt;
            }
            catch
            {
                TempData["AlertMessage"] = "查詢時發生錯誤";
            }
            return Page();
        }
        public EmployeeData GetEmployeeDataByEMPNo(string empno)
        {

            string strSQL = @"
SELECT EMP_No , 
               EMP_Name, 
               Role, 
               Department, 
               Position, 
               Agent, 
               Arrival_Date, 
               Job_Tenure, 
               Actual_Arrival_Date, 
               Sign_No, Status, 
               Resignation_Date, 
               Gender, 
               Date_Of_Birth, 
               ID, 
               Cellphone, 
               Telephone, 
               Residence_address, 
               Address, 
               Email, 
               Emergency_Contact, 
               Emergency_Contact_Phone 
  FROM EMP_Data WITH(NOLOCK)
WHERE EMP_No = @EMP_No
    ";

            SqlCommand command = Database.CreateCommand(strSQL);
            command.Parameters.AddWithValue("@EMP_No", empno);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new EmployeeData
                    {
                        EMP_No = reader["EMP_No"] as string,
                        EMP_Name = reader["EMP_Name"] as string,
                        Role = reader["Role"] as string,
                        Department = reader["Department"] as string,
                        Position = reader["Position"] as string,
                        Agent = reader["Agent"] is DBNull ? (string?)null : (string?)reader["Agent"],
                        Arrival_Date = reader["Arrival_Date"] is DBNull ? (DateTime?)null : (DateTime?)reader["Arrival_Date"],
                        Actual_Arrival_Date = reader["Actual_Arrival_Date"] is DBNull ? (DateTime?)null : (DateTime?)reader["Actual_Arrival_Date"],
                        Job_Tenure = reader["Job_Tenure"] is DBNull ? (int?)null : (int?)reader["Job_Tenure"],
                        Sign_No = reader["Sign_No"] is DBNull ? (string?)null : (string?)reader["Sign_No"],
                        Status = reader["Status"] is DBNull ? (string?)null : (string?)reader["Status"],
                        Resignation_Date = reader["Resignation_Date"] is DBNull ? (DateTime?)null : (DateTime?)reader["Resignation_Date"],
                        Gender = reader["Gender"] is DBNull ? (string?)null : (string?)reader["Gender"],
                        Date_Of_Birth = reader["Date_Of_Birth"] is DBNull ? (DateTime?)null : (DateTime?)reader["Date_Of_Birth"],
                        ID = reader["ID"] is DBNull ? (string?)null : (string?)reader["ID"],
                        Cellphone = reader["Cellphone"] is DBNull ? (string?)null : (string?)reader["Cellphone"],
                        Telephone = reader["Telephone"] is DBNull ? (string?)null : (string?)reader["Telephone"],
                        Residence_address = reader["Residence_address"] is DBNull ? (string?)null : (string?)reader["Residence_address"],
                        Address = reader["Address"] is DBNull ? (string?)null : (string?)reader["Address"],
                        Email = reader["Email"] is DBNull ? (string?)null : (string?)reader["Email"],
                        Emergency_Contact = reader["Emergency_Contact"] is DBNull ? (string?)null : (string?)reader["Emergency_Contact"],
                        Emergency_Contact_Phone = reader["Emergency_Contact_Phone"] is DBNull ? (string?)null : (string?)reader["Emergency_Contact_Phone"],
                    };
                }
                else
                {
                    // 如果找不到對應的資料，可以根據需求返回 null 或其他適當的值
                    return null;
                }
            }
        }
        public IActionResult OnGetEdit(string empno)
        {
            EmployeeData employeeData = GetEmployeeDataByEMPNo(empno);
            if (employeeData != null)
            {
                this.EMP_No = employeeData.EMP_No;
                this.EMP_Name = employeeData.EMP_Name;
                this.Role = employeeData.Role;
                this.Department = employeeData.Department;
                this.Position = employeeData.Position;
                this.Agent = employeeData.Agent;
                this.Arrival_Date = employeeData.Arrival_Date;
                this.Actual_Arrival_Date = employeeData.Actual_Arrival_Date;
                this.jobTenure = employeeData.Job_Tenure;
                this.Sign_No = employeeData.Sign_No;
                this.Status = employeeData.Status;
                this.Resignation_Date = employeeData.Resignation_Date;
                this.Gender = employeeData.Gender;
                this.Date_Of_Birth = employeeData.Date_Of_Birth;
                this.ID = employeeData.ID;
                this.Cellphone = employeeData.Cellphone;
                this.Telephone = employeeData.Telephone;
                this.Residence_address = employeeData.Residence_address;
                this.Address = employeeData.Address;
                this.Email = employeeData.Email;
                this.Emergency_Contact = employeeData.Emergency_Contact;
                this.Emergency_Contact_Phone = employeeData.Emergency_Contact_Phone;
            }

            return Page();
        }
        public IActionResult OnPostUpdated()
        {
            try
            {
                var jobTenureResult = OnPostCalculateJobTenure(new CalculateJobTenure { Actual_Arrival_Date = Actual_Arrival_Date ?? DateTime.MinValue });

                if (jobTenureResult.Value is JsonResult jsonResult && jsonResult.Value is JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty("jobTenure", out var jobTenureProperty))
                    {
                        jobTenure = jobTenureProperty.GetInt32();
                    }
                }


                string updatedEmployeeQuery = @"
UPDATE EMP_Data
         SET EMP_No = @EMP_No,
                 EMP_Name=@EMP_Name,
                 Role=@Role,
                 Department=@Department,
                 Position=@Position,
                 Agent=@Agent,
                Job_Tenure=@Job_Tenure,
                 Arrival_Date=@Arrival_Date,
                 Actual_Arrival_Date=@Actual_Arrival_Date,
                 Sign_No=@Sign_No,
                 Status =@Status,
                 Resignation_Date=@Resignation_Date,
                 Gender=@Gender,
                 Date_Of_Birth=@Date_Of_Birth,
                 ID=@ID,
                 Cellphone=@Cellphone,
                 Telephone=@Telephone,
                 Residence_address=@Residence_address,
                 Address=@Address,
                 Email=@Email,
                 Emergency_Contact=@Emergency_Contact,
                 Emergency_Contact_Phone =@Emergency_Contact_Phone,
                 Modifier = @Emp_No,
                 Modify_Date = GetDate()
  WHERE EMP_No = @EMP_No;
                    ";

                SqlCommand command = Database.CreateCommand(updatedEmployeeQuery);
                command.Parameters.AddWithValue("@EMP_No", EMP_No);
                command.Parameters.AddWithValue("@EMP_Name", EMP_Name);
                command.Parameters.AddWithValue("@Role", Role);
                command.Parameters.AddWithValue("@Department", Department);
                command.Parameters.AddWithValue("@Position", Position);
                command.Parameters.AddWithValue("@Agent", Agent);
                command.Parameters.AddWithValue("@Sign_No", Sign_No);
                command.Parameters.AddWithValue("@Job_Tenure", jobTenure);
                command.Parameters.AddWithValue("@Arrival_Date", (Object)Arrival_Date! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Actual_Arrival_Date", (Object)Actual_Arrival_Date! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Date_Of_Birth", (Object)Date_Of_Birth! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (Object)Status! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Gender", value: Gender);
                command.Parameters.AddWithValue("@ID", (Object)ID! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Cellphone", (Object)Cellphone! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Telephone", (Object)Telephone! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Residence_address", (Object)Residence_address! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (Object)Address! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (Object)Email! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Emergency_Contact", (Object)Emergency_Contact! ?? DBNull.Value);
                command.Parameters.AddWithValue("@Emergency_Contact_Phone", (Object)Emergency_Contact_Phone! ?? DBNull.Value);
                if (Status == "1") // 如果狀態為啟用
                {
                    command.Parameters.AddWithValue("@Resignation_Date", DBNull.Value); // 離職日為空
                }
                else if (Status == "2") // 如果狀態為停用
                {
                    if (Resignation_Date.HasValue) // 如果離職日有值
                    {
                        command.Parameters.AddWithValue("@Resignation_Date", Resignation_Date);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Resignation_Date", DateTime.Now); // 離職日為當下日期
                    }
                }
                command.ExecuteNonQuery();
                // Call Query
                // dt  存到 LIST 顯示 View.bag
                ViewData["QueryEmployeeData"] = this.QueryEmployeeData();
                TempData["AlertMessage"] = "更新完成";
                return RedirectToPage();
            }
            catch
            {
                TempData["AlertMessage"] = "更新時發生錯誤";
            }
            return Page();

        }
        
        public IActionResult OnPostReset()
        {
            string ResetPasswordQuery = @"
UPDATE EMP_Data
         SET Pass_Word = @EMP_No
  WHERE EMP_No = @EMP_No;
                    ";
            SqlCommand command = Database.CreateCommand(ResetPasswordQuery);
            command.ExecuteNonQuery();
            TempData["AlertMessage"] = "密碼已重置為員編";
            return Page();
        }
        public IActionResult OnPostClear()
        {
            ModelState.Clear(); 
            ModelState.Remove("EMP_No");
            ModelState.Remove("EMP_Name");
            ModelState.Remove("Role");
            ModelState.Remove("Department");
            ModelState.Remove("Position");
            ModelState.Remove("Agent");
            ModelState.Remove("Arrival_Date");
            ModelState.Remove("Actual_Arrival_Date");
            ModelState.Remove("Sign_No");
            ModelState.Remove("Status");
            ModelState.Remove("Resignation_Date");
            ModelState.Remove("Gender");
            ModelState.Remove("Date_Of_Birth");
            ModelState.Remove("ID");
            ModelState.Remove("Cellphone");
            ModelState.Remove("Telephone");
            ModelState.Remove("Residence_address");
            ModelState.Remove("Address");
            ModelState.Remove("Email");
            ModelState.Remove("Emergency_Contact");
            ModelState.Remove("Emergency_Contact_Phone");
            this.EMP_No = null;
            this.EMP_Name = null;
            this.Role = null;
            this.Department = null;
            this.Position = null;
            this.Agent = null;
            this.Arrival_Date = null;
            this.Actual_Arrival_Date = null;
            this.Sign_No = null;
            this.Status = null;
            this.Resignation_Date = null;
            this.Gender = null;
            this.Date_Of_Birth = null;
            this.ID = null;
            this.Cellphone = null;
            this.Telephone = null;
            this.Residence_address = null;
            this.Address = null;
            this.Email = null;
            this.Emergency_Contact = null;
            this.Emergency_Contact_Phone = null;
            ViewData["QueryEmployeeData"] = null;
            return Page();
        }
        public class CalculateJobTenure
        {
            public DateTime Actual_Arrival_Date { get; set; }
        }

        public JsonResult OnPostCalculateJobTenure([FromBody] CalculateJobTenure requestModel)
        {
            var jobTenureProperty = GetType().GetProperty("jobTenure", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            try
            {
                DateTime actualArrivalDate = requestModel.Actual_Arrival_Date;

                // 檢查日期合法性
                if (actualArrivalDate == null || DateTime.Now < actualArrivalDate)
                {
                    return new JsonResult(new { success = false, message = "請提供有效的到職日期" });
                }

                // 計算 jobTenure
                int jobTenure = 0;
                TimeSpan tenureSpan = DateTime.Now - actualArrivalDate;
                jobTenure = (int)tenureSpan.TotalDays + 1; // 加上一天

                // 將 jobTenure 設定到模型屬性中
                jobTenureProperty.SetValue(this, jobTenure);

                return new JsonResult(new { success = true, jobTenure = jobTenure });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "發生錯誤：" + ex.Message });
            }
        }

    }
}
