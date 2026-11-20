using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Pages.Account.SignupFlow
{
    public class Step1Model : PageModel
    {
        private readonly UserAuthenticationService _userAuthenticationService;
        public Step1Model(UserAuthenticationService userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            public string Email { get; set; }
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
      
        }       
        
        
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            return Page();
        }
    }
}
