using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using static HR_WorkFlow.Pages.Module.Form.LeaveModel;
using static HR_WorkFlow.Pages.Module.Manage.EmployeeModel;
using DataTable = System.Data.DataTable;

namespace HR_WorkFlow.Pages.Module.Manage
{
    [Authorize]
    public class EmpModel : PageModel
    {
        [BindProperty]
        public String? EMP_No { get; set; }

        public EmpModel(SqlDatabase database)
        {
        }

        public IActionResult OnPostClear()
        {
            ModelState.Clear();
            this.EMP_No ="dfdgh";
            return Page();
        }

        public void OnPost()
        {
            ModelState.Clear();
            // Set EmpNo to "TEST" when the form is submitted.
            EMP_No = "TEST";
        }


    }
}
