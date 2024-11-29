using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HR_WorkFlow.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
           return Redirect("Login");
        }
    }
}
