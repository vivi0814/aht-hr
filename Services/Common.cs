using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace HR_WorkFlow.Services
{
    public class Common
    {
        private SqlDatabase Database { get; set; }    //引用共用函式(SQLService)
        public Common(SqlDatabase database)   //注入服務(SQL&log)
        {
            Database = database;
        }
        /// <summary>
        /// 下拉式選單選項資料
        /// </summary>
        /// <param name="DDL_No">選單代碼</param>
        /// <param name="Trade_Code">交易代碼 可空白</param>
        /// <returns>選項代碼,選項名稱</returns>
        public DataTable GetDropdownOptions(string DDL_No, string Trade_Code)
        {
            string query = @"
SELECT Code_No,
       Code_Name
  FROM (SELECT '' AS Code_No,
               '＊全選' AS Code_Name,
	           0 AS Code_Sort
         UNION
        SELECT Code_No,
               Code_Name,
               Code_Sort
          FROM DDL_Setting WITH(NOLOCK)
         WHERE DDL_No = @DDL_No
           AND (Trade_Code LIKE @Trade_Code OR @Trade_Code = '')
       ) AS Temp
 ORDER BY Code_Sort
";
            if (!string.IsNullOrEmpty(Trade_Code))
            {
                Trade_Code = "%" + Trade_Code + "%";
            }
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@DDL_No", DDL_No);
            command.Parameters.AddWithValue("@Trade_Code", Trade_Code);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        /// <summary>
        /// 查詢權限名稱
        /// </summary>
        /// <param name="role">權限代碼</param>
        /// <returns>權限名稱</returns>
        public string GetRoleName(string role)
        {
            string query = @"
SELECT Code_Name
  FROM DDL_Setting WITH(NOLOCK)
 WHERE DDL_No = 'C01'
   AND Code_No=@Role
                        ";
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Role", role);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            string resultValue = dt.Rows[0]["Code_Name"].ToString() ?? "權限未定義";
            return resultValue;
        }
        /// <summary>
        /// 查詢員工姓名
        /// </summary>
        /// <param name="Emp_No">員工代號</param>
        /// <returns>員工姓名</returns>
        public string GetEmpNameName(string Emp_No)
        {
            string query = @"
SELECT Emp_Name
  FROM EMP_Data WITH(NOLOCK)
 WHERE Emp_No=@Emp_No
                        ";
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Emp_No", Emp_No);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            string resultValue = dt.Rows[0]["Emp_Name"].ToString() ?? "未登入";
            return resultValue;
        }
        /// <summary>
        /// 查詢簽核人與代理人
        /// </summary>
        /// <param name="Emp_No">傳入員工編號</param>
        /// <returns>員工編號,員工姓名</returns>
        public DataTable GetSingerData(string Emp_No)
        {
            DataTable dt = new DataTable();
            string query = @"
SELECT Signer,
       Emp_Name
  FROM dbo.VW_Signer_Data WITH(NOLOCK)
 WHERE Emp_No = @Emp_No
                            ";
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Emp_No", Emp_No);
            //DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }/// <summary>
         /// 讀取簽核人
         /// </summary>
         /// <param name="Emp_No">員工編號</param>
         /// <returns>回傳簽核人</returns>
        public DataTable GetSigner(string Emp_No)
        {
            string query = @"
SELECT Emp_no,
       Emp_Name
  FROM (SELECT 0 AS Seq,
               '' AS Emp_No,
               '' AS Emp_Name
         UNION ALL
        SELECT 1 AS Seq,
               ENo.Sign_No AS Emp_NO,
               SNo.EMP_Name
          FROM EMP_Data AS ENo
          LEFT JOIN EMP_Data AS SNo ON ENo.Sign_No = SNo.EMP_No 
         WHERE ENo.EMP_NO= @Emp_No
         UNION ALL
        SELECT 2 AS Seq,
               ENo.Agent AS Emp_No,
               '(代)'+ ANo.EMP_Name
          FROM EMP_Data AS ENo
          LEFT JOIN EMP_Data AS ANo ON ENo.Agent = ANo.EMP_No 
         WHERE ENo.EMP_NO= @Emp_No) AS Temp
 ORDER BY Seq
                        ";
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@Emp_No", Emp_No);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        /// <summary>
        /// 計算請假時數
        /// </summary>
        /// <param name="LeaveStartTime">請起日時間</param>
        /// <param name="LeaveEndTime">請假迄日時間</param>
        /// <returns>回傳天數 小時數</returns>
        public DataTable GetLeavehours(string LeaveStartTime, string LeaveEndTime)
        {
            DateTime startTime = DateTime.Parse(LeaveStartTime);
            DateTime endTime = DateTime.Parse(LeaveEndTime);
            if (LeaveStartTime != null && LeaveEndTime != null && startTime <= endTime) { } 
            string query = @"
DECLARE @WorkStartTime TIME    -- 上班時間
DECLARE @WorkEndTime TIME      -- 下班時間
DECLARE @LunchStartTime TIME   -- 午休開始時間
DECLARE @LunchEndTime TIME     -- 午休結束時間
SELECT @WorkStartTime = Work_Start_Time,
       @WorkEndTime = Work_End_Time,
	   @LunchStartTime = Lunch_Start_Time,
	   @LunchEndTime = Lunch_End_Time
  FROM Work_Time_Setting WITH(NOLOCK)
DECLARE @LeaveStartDate DATETIME = @LeaveStartTime; -- 請假起始日期
DECLARE @LeaveEndDate DATETIME = @LeaveEndTime;   -- 請假結束日期
-- 計算1天的上班時間  480分鐘
DECLARE @DailyWorkMinutes INT;
SET @DailyWorkMinutes = DATEDIFF(MINUTE, @WorkStartTime, @WorkEndTime) - DATEDIFF(MINUTE, @LunchStartTime, @LunchEndTime);
-- 計算請假期間的總分鐘數
--計算總共幾天 計算請假時間的總分鐘數 
DECLARE @TotalMinutes INT;
SET @TotalMinutes =DATEDIFF(DAY,@LeaveStartDate, @LeaveEndDate) + 1		--總共幾天
SET @TotalMinutes = @TotalMinutes * @DailyWorkMinutes
--扣掉上班時間 與請假開始時間
DECLARE @WORKLeaveMinutes INT 
--請假起日是否有滿八小時 判斷時間有沒有超過中午
DECLARE @StartTime INT
IF CONVERT(TIME,@LeaveStartDate) <= @LunchStartTime   --起始時間是否在上午
BEGIN
	SET @StartTime = DATEDIFF(MINUTE,@WorkStartTime,CONVERT(TIME,@LeaveStartDate))	
END
ELSE
BEGIN    --起始時間從下午開始
	SET @StartTime = DATEDIFF(MINUTE,@LunchEndTime,CONVERT(TIME,@LeaveStartDate))
END
--PRINT @StartTime
SET @TotalMinutes = @TotalMinutes - @StartTime
--PRINT @TotalMinutes
--計算結束日那天有滿八個小時
DECLARE @EndTime INT
IF CONVERT(TIME,@LeaveEndDate) <= @LunchEndTime   --結束時間是否在上午
BEGIN
	SET @EndTime = DATEDIFF(MINUTE,CONVERT(TIME,@LeaveEndDate),@WorkEndTime) - DATEDIFF(MINUTE, @LunchStartTime, @LunchEndTime);
	--PRINT @ENDTIME
	--PRINT DATEDIFF(MINUTE,CONVERT(TIME,@LeaveEndDate),@WorkEndTime)
	--PRINT DATEDIFF(MINUTE, @LunchStartTime, @LunchEndTime);
END
ELSE
BEGIN    --結束時間從下午開始
	SET @EndTime = DATEDIFF(MINUTE,CONVERT(TIME,@LeaveEndDate),@WorkEndTime)
END
	--PRINT @ENDtiME
SET @TotalMinutes = @TotalMinutes - @EndTime
--扣掉假日及國定假日
DECLARE @Holiday INT
SELECT @Holiday = COUNT(*)
  FROM Calendar WITH(NOLOCK)
 WHERE CONVERT(VARCHAR,HR_DAY,112) BETWEEN CONVERT(VARCHAR,@LeaveStartDate,112) AND CONVERT(VARCHAR,@LeaveEndDate,112)
   AND TYPE IN (1,2,3)
DECLARE @HolidayMinutes INT
SET @HolidayMinutes =  @Holiday * 8 * 60
--PRINT @HolidayMinutes 
--print @TotalMinutes
SET @TotalMinutes = @TotalMinutes - @HolidayMinutes
-- 計算請假時間的小時
DECLARE @TotalHours DECIMAL(10,2)
DECLARE @TotalLeaveHours DECIMAL(18, 2);
DECLARE @RemainingMinutes INT;
-- 計算請假時間的分鐘
SET @TotalHours = @TotalMinutes /60
--PRINT @TotalHours
SET @RemainingMinutes = @TotalMinutes % 480   --計算一天餘數 
--PRINT @RemainingMinutes
SET @RemainingMinutes = @RemainingMinutes %60 --計算一小時餘數
--PRINT @RemainingMinutes
IF @RemainingMinutes> 1 AND @RemainingMinutes < = 30
BEGIN
	SET @TotalLeaveHours=@TotalHours + 0.5
END
ELSE IF @RemainingMinutes > 30 AND @RemainingMinutes < 60
BEGIN
	SET @TotalLeaveHours = @TotalHours + 1
END
ELSE
BEGIN
	SET @TotalLeaveHours = @TotalHours
END
--PRINT @TotalLeaveHours
DECLARE @LeaveDay INT
SET @LeaveDay = @TotalLeaveHours / 8
--PRINT @LeaveDay
DECLARE @LeaveHour DECIMAL(10,1)
SET @LeaveHour = @TotalLeaveHours % 8
--PRINT @LeaveHour
SELECT @LeaveDay AS Days,
       @LeaveHour AS Hours
                        ";
            SqlCommand command = Database.CreateCommand(query);
            command.Parameters.AddWithValue("@LeaveStartTime", startTime);
            command.Parameters.AddWithValue("@LeaveEndTime", endTime);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        /// <summary>
        /// 讀取單號
        /// </summary>
        /// <returns>回傳單號</returns>
        public DataTable GetLeaveNo()
        {
            string query = @"

SELECT CASE WHEN COUNT(*) = 0 THEN LEFT(CONVERT(VARCHAR,GETDATE(),112),6) + '001'
            ELSE LEFT(CONVERT(VARCHAR,GETDATE(),112),6) + FORMAT(ISNULL(CONVERT(INT, RIGHT(MAX(Leave_NO), 3)), 0) + 1, '000')
			END
  FROM Leave_Overtime WITH(NOLOCK)
 WHERE LEFT(Leave_No,6) = LEFT(CONVERT(VARCHAR,GETDATE(),112),6)
                        ";
            SqlCommand command = Database.CreateCommand(query);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
        public DataTable GetDropdowEmpList()
        {
            string query = @"
SELECT '' AS Code_No,
       '＊全選' AS Code_Name,
    0 AS Code_Sort 
UNION
SELECT EMP_No AS Code_No,
       EMP_Name AS Code_Name,
   ROW_NUMBER() OVER(ORDER BY EMP_No) AS Code_Sort
  FROM EMP_Data WITH(NOLOCK)
 WHERE Status = '1'
";
            SqlCommand command = Database.CreateCommand(query);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dt);
            return dt;
        }
    }
}
