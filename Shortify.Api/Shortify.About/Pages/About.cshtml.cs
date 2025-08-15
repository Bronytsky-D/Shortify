using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Shortify.About.Pages
{
    public class AboutModel : PageModel
    {
        public string Description { get; set; } = @"The URL shortening algorithm generates a unique short code for any web address.
            1. Validates the URL (http/https).
            2. Generates a random Base62 code, 6 characters long.
            3. Checks the database for code uniqueness; retries up to 6 times if a conflict occurs.
            4. Stores the record with the original URL, short code, and user (if any).
            5. To retrieve the URL, searches the code in the database; returns an error if not found.";

        public bool IsAdmin { get; set; }

        public void OnGet()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            IsAdmin = role == "Admin";
        }

        public IActionResult OnPost()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var updatedDescription = Request.Form["Description"];
            if (!string.IsNullOrEmpty(updatedDescription))
            {
                Description = updatedDescription;
            }

            return Page();
        }
    }
}
