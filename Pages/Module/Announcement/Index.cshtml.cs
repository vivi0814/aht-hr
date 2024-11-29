using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HR_WorkFlow.Pages.Module.Announcement
{
    [Authorize]
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }   //前端欄位綁定

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public IFormFile? ImportedFile { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly Excel _excel;

        public IndexModel(ILogger<IndexModel> logger, Excel excel)
        {
            _logger = logger;
            _excel = excel;
        }

        public void OnGet()
        {

        }

        public void OnPostAdd()
        {
            TempData["Action"] = "Add";
        }

        public void OnPostAddConfirm()
        {
            TempData["AlertMessage"] = "新增成功";
        }
        public void OnPostQuery() {
			TempData["AlertMessage"] = "查詢成功";
		}

        public void OnPostImport()
        {
            _logger.LogInformation("Importing...");
            
            if (ImportedFile == null)
            {
                TempData["AlertMessage"] = "請選擇檔案";
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                ImportedFile.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var ds = _excel.ImportFromExcel(fileBytes);
            }
        }
    }
}