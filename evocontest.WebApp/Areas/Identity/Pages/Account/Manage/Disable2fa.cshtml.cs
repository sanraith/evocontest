using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evocontest.WebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace evocontest.WebApp.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

        public Disable2faModel(
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            return RedirectToPage("~");
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //}

            //if (!await _userManager.GetTwoFactorEnabledAsync(user))
            //{
            //    throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");
            //}

            //return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return RedirectToPage("~");
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //}

            //var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            //if (!disable2faResult.Succeeded)
            //{
            //    throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            //}

            //_logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
            //StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            //return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}