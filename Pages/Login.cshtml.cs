using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Safar.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Email == "admin@safar.com" && Password == "safar123")
            {
                return RedirectToPage("/Admin/Dashboard");
            }
            ErrorMessage = "Authentication failed. Invalid system credentials.";
            return Page();
        }
    }
}