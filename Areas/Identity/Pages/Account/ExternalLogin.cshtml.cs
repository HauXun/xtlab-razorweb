
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using App.Models;

namespace App.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class ExternalLoginModel : PageModel
  {
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserStore<AppUser> _userStore;
    private readonly IUserEmailStore<AppUser> _emailStore;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IUserStore<AppUser> userStore,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _userStore = userStore;
      _emailStore = GetEmailStore();
      _logger = logger;
      _emailSender = emailSender;
    }


    [BindProperty]
    public InputModel Input { get; set; }


    public string ProviderDisplayName { get; set; }


    public string ReturnUrl { get; set; }


    [TempData]
    public string ErrorMessage { get; set; }


    public class InputModel
    {

      [Required]
      [EmailAddress]
      public string Email { get; set; }
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
      // Request a redirect to the external login provider.
      var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");
      if (remoteError != null)
      {
        ErrorMessage = $"L???i t??? d???ch v??? ngo??i: {remoteError}";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        ErrorMessage = "Kh??ng l???y ???????c th??ng tin t??? d???ch v??? ngo??i";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      // Sign in the user with this external login provider if the user already has a login.
      var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
      if (result.Succeeded)
      {
        _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
        return LocalRedirect(returnUrl);
      }
      if (result.IsLockedOut)
      {
        return RedirectToPage("./Lockout");
      }
      else
      {
        // If the user does not have an account, then ask the user to create an account.
        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
          Input = new InputModel
          {
            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
          };
        }
        return Page();
      }
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");
      // Get the information about the user from the external login provider
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        ErrorMessage = "L???i l???y th??ng tin t??? d???ch v??? ngo??i";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      if (ModelState.IsValid)
      {
        var registeredUser = await _userManager.FindByEmailAsync(Input.Email);
        string externalEmail = null;
        AppUser externalEmailUser = null;

        // Claim ~ Dac tinh mo ta mot doi tuong
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
          externalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
        }

        if (externalEmail != null)
        {
          externalEmailUser = await _userManager.FindByEmailAsync(externalEmail);
        }

        if ((registeredUser != null) && (externalEmailUser != null))
        {
          if (registeredUser.Id == externalEmailUser.Id)
          {
            var resultLink = await _userManager.AddLoginAsync(registeredUser, info);
            if (resultLink.Succeeded)
            {
              await _signInManager.SignInAsync(registeredUser, isPersistent: false);
              return LocalRedirect(returnUrl);
            }
          }
          else
          {
            ModelState.AddModelError(string.Empty, "Kh??ng li??n k???t ???????c t??i kho???n, h??y s??? d???ng t??i kho???n kh??c");
            return Page();
          }
        }

        if ((registeredUser == null) && (externalEmailUser != null))
        {
          ModelState.AddModelError(string.Empty, "Kh??ng h??? tr??? t???o t??i kho???n m???i - c?? email kh??c email t??? d???ch v??? ngo??i");
          return Page();
        }

        if ((externalEmailUser == null) && (externalEmail == Input.Email))
        {
          var newUser = new AppUser
          {
            UserName = externalEmail,
            Email = externalEmail
          };

          var resultNewUser = await _userManager.CreateAsync(newUser);
          if (resultNewUser.Succeeded)
          {
            await _userManager.AddLoginAsync(newUser, info);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await _userManager.ConfirmEmailAsync(newUser, code);

            await _signInManager.SignInAsync(newUser, isPersistent: false);

            return LocalRedirect(returnUrl);
          }
          else
          {
            ModelState.AddModelError(string.Empty, "Kh??ng t???o ???????c t??i kho???n m???i");
            return Page();
          }
        }

        var user = CreateUser();

        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
          result = await _userManager.AddLoginAsync(user, info);
          if (result.Succeeded)
          {
            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
              return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
            }

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            return LocalRedirect(returnUrl);
          }
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      ProviderDisplayName = info.ProviderDisplayName;
      ReturnUrl = returnUrl;
      return Page();
    }

    private AppUser CreateUser()
    {
      try
      {
        return Activator.CreateInstance<AppUser>();
      }
      catch
      {
        throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
            $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
            $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
      }
    }

    private IUserEmailStore<AppUser> GetEmailStore()
    {
      if (!_userManager.SupportsUserEmail)
      {
        throw new NotSupportedException("The default UI requires a user store with email support.");
      }
      return (IUserEmailStore<AppUser>)_userStore;
    }
  }
}
