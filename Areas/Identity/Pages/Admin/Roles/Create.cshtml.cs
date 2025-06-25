using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MiniAccountsSystem.Areas.Identity.Pages.Admin.Roles
{
    [Authorize(Roles = "Admin")] // Only Admin users can access this page
    public class CreateModel : PageModel
    {
       
      
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ILogger<CreateModel> _logger;

            public CreateModel(RoleManager<IdentityRole> roleManager, ILogger<CreateModel> logger)
            {
                _roleManager = roleManager;
                _logger = logger;
            }

            [BindProperty]
            public InputModel Input { get; set; }

            public class InputModel
            {
                [Required]
                [Display(Name = "Role Name")]
                [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
                public string RoleName { get; set; }
            }

            public IActionResult OnGet()
            {
                return Page();
            }

            public async Task<IActionResult> OnPostAsync()
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var role = new IdentityRole(Input.RoleName);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{Input.RoleName}' created successfully.");
                    return RedirectToPage("./Index"); // Redirect back to roles list
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }
        }
    }