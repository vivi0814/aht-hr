using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace HR_WorkFlow.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }   //前端欄位綁定

        [BindProperty]
        public string? Password { get; set; }

		[BindProperty]
		public string? Message { get; set; }


		protected SqlDatabase Database { get; set; }    //引用共用函式(SQLService)
        protected Common Common { get; set; }   

        protected ILogger<LoginModel> Logger { get; set; }  //引用共用函式(log)

        public LoginModel(SqlDatabase database, ILogger<LoginModel> logger)   //注入服務(SQL&log) 
        {
            Database = database;
            Logger = logger;
        }

        public void OnGet()             
        {
                        //畫面預設值可放在這
        }

        public IActionResult OnPost()
        {
            string query = @"SELECT EMP_NAME,
                                    Role,
                                    EMP_No,
                                    Login_Cnt,
                                    DATEDIFF(MINUTE,isNull(Fail_Login_Time,GETDATE()),GETDATE()) AS Fail_Time 
                               FROM EMP_Data 
                              WHERE EMP_No = @Username 
                                AND Pass_Word = @Password";
            SqlCommand command = Database.CreateCommand(query);
            if (String.IsNullOrEmpty(Username)) { 
                Username = string.Empty;
            }
			if (String.IsNullOrEmpty(Password))
			{
				Password = string.Empty;
			}
			command.Parameters.AddWithValue("@Username", Username);
            command.Parameters.AddWithValue("@Password", Password);
            
            // get the user and role from the database
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            
            if (dt.Rows.Count > 0)
            {
                string user = dt.Rows[0]["EMP_NAME"].ToString()!;
                string role = dt.Rows[0]["Role"].ToString()!;
                string EMP_No = dt.Rows[0]["EMP_No"].ToString()!;
                int Login_Cnt = Convert.ToInt32(dt.Rows[0]["Login_Cnt"].ToString())!;
                int Fail_Time = Convert.ToInt32(dt.Rows[0]["Fail_Time"].ToString())!;
                if (Login_Cnt <= 3)
                {
                    // create the identity for the user
                    var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, EMP_No),
                    new Claim(ClaimTypes.Role, role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                    //更新登入時間並將登入次數更改為0
                    string UpdateSQL = @"UPDATE EMP_Data 
                                        SET Login_Cnt = '0',
                                            Login_Time = GETDATE()
                                      WHERE EMP_No = @EMP_No";
                    SqlCommand UPdateCommand = Database.CreateCommand(UpdateSQL);
                    UPdateCommand.Parameters.AddWithValue("@EMP_No", EMP_No);
                    UPdateCommand.ExecuteNonQuery();
                    // sign in
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                    return Redirect("~/Announcement/Index");
                }
                else {
                    if (Fail_Time >=30) {

						string UpdateSQL = @"UPDATE EMP_Data 
                                                SET Login_Cnt = '0',
                                                    Login_Time = GETDATE()
                                              WHERE EMP_No = @EMP_No";
						SqlCommand UPdateCommand = Database.CreateCommand(UpdateSQL);
						UPdateCommand.Parameters.AddWithValue("@EMP_No", EMP_No);
						UPdateCommand.ExecuteNonQuery();

					}
                    return Page();
				}
			}
            else
            {
                //記錄此帳號錯誤次數
                //查詢登入帳號是否存在
                string strErrorSQL = @"SELECT EMP_No,
                                              Login_Cnt
                                         FROM EMP_Data
                                        WHERE EMP_No = @EMP_No
                                      ";
				SqlCommand ErrorCommand = Database.CreateCommand(strErrorSQL);
				if (String.IsNullOrEmpty(Username))
				{
					Username = string.Empty;
				}
				ErrorCommand.Parameters.AddWithValue("@EMP_No", Username);
				DataTable ErrorDt = new DataTable();
				SqlDataAdapter ErrorDa = new SqlDataAdapter(ErrorCommand);
				ErrorDa.Fill(ErrorDt);

				//更新錯誤次數及錯誤時間
				if (ErrorDt.Rows.Count > 0)
                {
					string ErrorEMPNo = ErrorDt.Rows[0]["EMP_No"].ToString()!;
					int ErrorLoginCnt = Convert.ToInt32(ErrorDt.Rows[0]["Login_Cnt"].ToString())!;
					string UpdateSQL = @"UPDATE EMP_Data 
                                            SET Login_Cnt = @LoginCnt,
                                                Fail_Login_Time = GETDATE()
                                          WHERE EMP_No = @EMP_No";
					SqlCommand ErrorEditCommand = Database.CreateCommand(UpdateSQL);
					ErrorEditCommand.Parameters.AddWithValue("@EMP_No", ErrorEMPNo);
                    ErrorEditCommand.Parameters.AddWithValue("@LoginCnt", ErrorLoginCnt+1);
					ErrorEditCommand.ExecuteNonQuery();

				}
                //查無資料不處理
					

				Logger.LogInformation($"User {Username} failed to login.");
                TempData["AlertMessage"] = "帳號或密碼錯誤";
                return Page();
			}


            /**


            if (command.ExecuteScalar() is null)
            {
                Logger.LogInformation($"User {Username} failed to login.");
                return RedirectToPage("Login");
            }
            else
            {
                Logger.LogInformation($"User {Username} logged in.");
                HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Username!),  //存放使用者資訊
                    new Claim(ClaimTypes.Role, "Admin"),    
                    new Claim("UserType", "Admin")          //存放自建使用者資訊
                }, CookieAuthenticationDefaults.AuthenticationScheme)));
                HttpContext.Session.SetString("UserRole", );
                return Redirect("~/Announcement/Index");
            }
            */
        }
    }
}
