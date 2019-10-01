using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using evocontest.WebApp.Data;
using evocontest.WebApp.Data.Helper;
using evocontest.WebApp.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace evocontest.WebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly Dictionary<string, string> _emailToRoleMap;
        private static string[] _validEmailDomains;
        private static string[] _validEmailDomainsExtended;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;

            _emailToRoleMap = new Dictionary<string, string>
            {
                { configuration.GetValue<string>("AdminEmail").ToLowerInvariant(), Roles.Admin},
                { configuration.GetValue<string>("WorkerEmail").ToLowerInvariant(), Roles.Worker}
            };
            _validEmailDomains = configuration.GetValue<string>("ValidEmailDomains").Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            _validEmailDomainsExtended = _validEmailDomains.Concat(_emailToRoleMap.Keys).ToArray();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel : IValidatableObject
        {
            [Required]
            [StringLength(100, ErrorMessage = "A {0} hossza {2} - {1} karakter kell legyen.", MinimumLength = 1)]
            [Display(Name = "Vezetéknév")]
            public string LastName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "A {0} hossza {2} - {1} karakter kell legyen.", MinimumLength = 1)]
            [Display(Name = "Keresztnév")]
            public string FirstName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "A {0} hossza {2} - {1} karakter kell legyen.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Jelszó")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Jelszó megerősítése")]
            [Compare("Password", ErrorMessage = "A jelszó mezők tartalma nem egyezik meg.")]
            public string ConfirmPassword { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (Email == null) { yield break; }

                if (_validEmailDomainsExtended.Any(suffix => Email.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return ValidationResult.Success;
                }
                else
                {
                    yield return new ValidationResult($"Használd az evosoftos email címed! Elfogadott domainek: {string.Join(", ", _validEmailDomains)}");
                }
            }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { FirstName = Input.FirstName, LastName = Input.LastName, UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (_emailToRoleMap.TryGetValue(user.Email.ToLowerInvariant(), out var mappedRole))
                    {
                        _logger.LogInformation($"User was mapped to role: {mappedRole}");
                        await _userManager.AddToRoleAsync(user, mappedRole);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    var emailContent = string.Format(StringLibrary.EmailConfirmationContent, HtmlEncoder.Default.Encode(callbackUrl));
                    await _emailSender.SendEmailAsync(Input.Email, StringLibrary.EmailConfirmationSubject, emailContent);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
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
            return Page();
        }
    }
}
