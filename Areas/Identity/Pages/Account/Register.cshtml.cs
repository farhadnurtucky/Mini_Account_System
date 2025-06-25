using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this namespace for SelectList
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient; // Add this for SqlParameter
using Microsoft.EntityFrameworkCore; // Add this for ExecuteSqlRawAsync
using Microsoft.Extensions.Logging;
using MiniAccountsSystem.Data; // Ensure this is your DbContext's namespace

namespace MiniAccountsSystem.Areas.Identity.Pages.Account // Your project's namespace
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context; // Add this
        private readonly RoleManager<IdentityRole> _roleManager; // Add this

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager, // Add this to constructor
            ApplicationDbContext context) // Add this to constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager; // Assign injected RoleManager
            _context = context; // Assign injected ApplicationDbContext
        }

        // Dropdown options for roles
        public SelectList DropdownOption { get; set; } // Add this property

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Add this property for selected role ID
            [Required(ErrorMessage = "Please select a role.")]
            [Display(Name = "Role")]
            public string SelectedRoleId { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            // Populate the DropdownOption with roles on page load
            // Ensure roles exist in the database (via seeding or manual creation)
            DropdownOption = new SelectList(_roleManager.Roles.ToList(), "Id", "Name");

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Re-populate DropdownOption in case of ModelState invalidation,
            // so the dropdown is not empty if the form needs to be re-displayed.
            DropdownOption = new SelectList(_roleManager.Roles.ToList(), "Id", "Name");

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    try
                    {
                        // Validate SelectedRoleId before using it
                        if (string.IsNullOrEmpty(Input.SelectedRoleId))
                        {
                            _logger.LogError("SelectedRoleId is null or empty. Cannot assign role.");
                            ModelState.AddModelError(string.Empty, "Role selection is missing. Please select a role.");
                            await _userManager.DeleteAsync(user); // Optionally delete user if role assignment is critical
                            return Page();
                        }

                        // Execute the stored procedure to assign the role
                        _logger.LogInformation($"Attempting to assign role {Input.SelectedRoleId} to user {user.Id}");

                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC sp_MiniAccount_AssignRight @userId, @roleId",
                            new SqlParameter("@userId", user.Id),
                            new SqlParameter("@roleId", Input.SelectedRoleId) // Use SelectedRoleId from InputModel
                        );

                        _logger.LogInformation($"Role {Input.SelectedRoleId} assigned successfully to user {user.Id}");
                    }
                    catch (SqlException ex)
                    {
                        // Log the specific SQL error for debugging
                        _logger.LogError(ex, $"SQL Error occurred while assigning role to user {user.Id} with role {Input.SelectedRoleId}: {ex.Message}");
                        ModelState.AddModelError(string.Empty, "An error occurred while assigning the role. Please try again later.");
                        await _userManager.DeleteAsync(user); // Consider rolling back user creation if role assignment is critical
                        return Page();
                    }
                    catch (Exception ex)
                    {
                        // Log any other unexpected errors during role assignment
                        _logger.LogError(ex, $"An unexpected error occurred during role assignment for user {user.Id}: {ex.Message}");
                        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                        await _userManager.DeleteAsync(user); // Consider rolling back user creation
                        return Page();
                    }

                    // Email confirmation logic (wrapped in try-catch)
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    try
                    {
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending email confirmation to {Input.Email}: {ex.Message}");
                        ModelState.AddModelError(string.Empty, "An error occurred while sending the confirmation email. Please try again later.");
                        // Decided to allow flow to continue as email confirmation might not be critical for initial access,
                        // but you could return Page() here if it is.
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            // DropdownOption is re-populated at the beginning of OnPostAsync
            return Page();
        }
    }
}