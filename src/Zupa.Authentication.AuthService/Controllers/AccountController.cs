using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.EmailSending;
using Zupa.Authentication.AuthService.Exceptions;
using Zupa.Authentication.AuthService.Models.Account;
using Zupa.Authentication.AuthService.Services;
using Zupa.Authentication.AuthService.Services.FailedLoginAttempts;
using Zupa.Authentication.AuthService.Services.Registration;

namespace Zupa.Authentication.AuthService.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly AccountService _account;
        private readonly LockoutSettings _lockoutSettings;
        private readonly ISendEmail _emailSender;
        private readonly TransactionalTemplateConfiguration _transactionalTemplateConfiguration;
        private readonly ITrackTelemetry _trackTelemetry;
        private readonly MessageConstants _messageConstants;
        private readonly IRegisterService _registerService;
        private readonly IFailedLoginAttemptsService _failedLoginAttemptsService;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IHostingEnvironment env,
            IIdentityServerInteractionService interaction,
            IHttpContextAccessor httpContextAccessor,
            ISendEmail emailSender,
            IOptions<TransactionalTemplateConfiguration> transactionalTemplateConfiguration,
            ILogger<AccountController> logger,
            IOptions<LockoutSettings> lockoutSettings,
            ITrackTelemetry trackTelemetry,
            IOptions<MessageConstants> messageConstants,
            IRegisterService registerService,
            IFailedLoginAttemptsService failedLoginAttemptsService)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _transactionalTemplateConfiguration = transactionalTemplateConfiguration?.Value
                                                  ?? throw new ArgumentNullException(nameof(transactionalTemplateConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lockoutSettings = lockoutSettings?.Value ?? throw new ArgumentNullException(nameof(lockoutSettings));
            _account = new AccountService(interaction, httpContextAccessor);
            _trackTelemetry = trackTelemetry ?? throw new ArgumentNullException(nameof(trackTelemetry));
            _messageConstants = messageConstants.Value;
            _registerService = registerService;
            _failedLoginAttemptsService = failedLoginAttemptsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendEmailConfirmation(user, returnUrl);

                    if (!Guid.TryParse(user.Id, out var userId))
                        throw new FormatException($"{nameof(user.Id)} could not be parsed as a Guid");

                    await _registerService.SendUserRegistrationAsync(userId, user.UserName, user.Email);

                    _trackTelemetry.TrackEvent(EventName.Registration, EventType.Method, Providers.Zupa);

                    return View(nameof(ConfirmEmail));
                }
                AddErrors(result);
            }

            return View(model);
        }

        private async Task SendEmailConfirmation(IdentityUser user, string returnUrl = null)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var redirectUri = returnUrl != null
                ? new Uri(HttpUtility.ParseQueryString(Url.Content(returnUrl)).Get("redirect_uri"))
                    .GetLeftPart(UriPartial.Authority)
                : null;

            var callbackUrl = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new { userId = user.Id, code, redirectUri },
                protocol: Request.Scheme);

            var outgoingEmail = new OutgoingEmail
            {
                To = user.Email,
                From = _transactionalTemplateConfiguration.FromEmail,
                Template = new Template
                {
                    Id = _transactionalTemplateConfiguration.RegistrationTemplate,
                    Substitutions = new Dictionary<string, string>
                    {
                        {_transactionalTemplateConfiguration.DynamicUserNameKey, user.UserName},
                        {_transactionalTemplateConfiguration.DynamicLinkKey, callbackUrl}
                    },
                    IsDynamic = true
                }
            };

            await _emailSender.SendEmailAsync(outgoingEmail);
        }

        private async Task SendLockoutEmail(IdentityUser user)
        {
            var outgoingEmail = new OutgoingEmail
            {
                To = user.Email,
                From = _transactionalTemplateConfiguration.FromEmail,
                Template = new Template
                {
                    Id = _transactionalTemplateConfiguration.LockoutTemplate,
                    Substitutions = new Dictionary<string, string>
                    {
                        {_transactionalTemplateConfiguration.UserNameKey, user.UserName},
                        {_transactionalTemplateConfiguration.LockoutTimeKey, _lockoutSettings.DefaultLockoutTimeSpan.ToString()}
                    },
                    IsDynamic = false
                }
            };

            await _emailSender.SendEmailAsync(outgoingEmail);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string redirectUri = null)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(Login));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest($"Unable to load user with ID '{userId}'.");
            }

            if (user.EmailConfirmed)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                var outgoingEmail = new OutgoingEmail
                {
                    To = user.Email,
                    From = _transactionalTemplateConfiguration.FromEmail,
                    Template = new Template
                    {
                        Id = _transactionalTemplateConfiguration.WelcomeTemplate,
                        Substitutions = new Dictionary<string, string>
                        {
                            {_transactionalTemplateConfiguration.UserNameKey, user.UserName}
                        },
                        IsDynamic = false
                    }
                };

                await _emailSender.SendEmailAsync(outgoingEmail);
            }

            if (redirectUri != null)
                return Redirect(redirectUri);
            else
                return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            ViewData["TooManyRequests"] = false;
            ViewData["ReturnUrl"] = returnUrl;
            
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            if (ipAddress == null)
                throw new IpAddressNotFoundException();
         
            var tooManyRequestsForIpAddress = await _failedLoginAttemptsService.HasTooManyRequestsForIpAddressAsync(ipAddress.ToString());
            ViewData["TooManyRequests"] = tooManyRequestsForIpAddress;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoggedOut(LoggedOutViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var viewModel = await _account.BuildLogoutViewModelAsync(logoutId);

            if (!viewModel.ShowLogoutPrompt)
            {
                return await Logout(viewModel);
            }
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            if (ModelState.IsValid)
            {
                var viewModel = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User logged out.");

                if (viewModel.TriggerExternalSignOut)
                {
                    string url = Url.Action(nameof(Logout), new { logoutId = viewModel.LogoutId });

                    return SignOut(new AuthenticationProperties { RedirectUri = url }, viewModel.ExternalAuthenticationScheme);
                }

                return View(nameof(LoggedOut), viewModel);
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            ViewData["TooManyRequests"] = false;
            
            if (model.Email == null || model.Password == null)
                return View(model);

            if (ipAddress == null)
                throw new IpAddressNotFoundException();
            
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.UserNotFound);

                ModelState.AddModelError(string.Empty, _messageConstants.InvalidLoginAttempt);

                await _failedLoginAttemptsService.HandleFailedLoginAttempt(ipAddress.ToString());
                var tooManyRequestsForIpAddress = await _failedLoginAttemptsService.HasTooManyRequestsForIpAddressAsync(ipAddress.ToString());
                if (tooManyRequestsForIpAddress)
                    return RedirectToAction(nameof(Login));
                
                return View(model);
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                await _failedLoginAttemptsService.HandleFailedLoginAttempt(ipAddress.ToString());
                var tooManyRequestsForIpAddress = await _failedLoginAttemptsService.HasTooManyRequestsForIpAddressAsync(ipAddress.ToString());
                if (tooManyRequestsForIpAddress)
                    return RedirectToAction(nameof(Login));
                
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.Success);
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    await SendLockoutEmail(user);
                    _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.Lockout);
                    _logger.LogWarning("The user has been locked out after too many failed login attempts");
                }

                if (result.IsNotAllowed)
                {
                    _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.NotVerified);
                }

                ModelState.AddModelError(string.Empty, _messageConstants.InvalidLoginAttempt);
            }

            _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.Fail);
            
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl, string remoteError = null)
        {
            if (remoteError != null)
            {
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            Enum.TryParse<Providers>(info.ProviderDisplayName, true, out var provider);

            _trackTelemetry.TrackEvent(EventName.Login, EventType.Method, provider);

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.Success);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsNotAllowed)
            {
                _trackTelemetry.TrackEvent(EventName.Login, EventType.Action, EventStatus.NotVerified);
                return RedirectToAction(nameof(Login));
            }
            _trackTelemetry.TrackEvent(EventName.Registration, EventType.Method, provider);
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            return View(nameof(ExternalLogin), new ExternalLoginModel { Email = info.Principal.FindFirstValue(ClaimTypes.Email) });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return BadRequest("Error loading external login information during confirmation.");
                }
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await SendEmailConfirmation(user, returnUrl);
                    return View(nameof(ConfirmEmail));
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Login), "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordConfirm(ForgotPasswordModel model)
        {
            if (string.IsNullOrEmpty(model?.Email))
                return View(nameof(ForgotPassword));

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.UserNotFound);

                    // Don't reveal that the user does not exist or is not confirmed
                    return View(nameof(ForgotPasswordConfirm));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Method, Providers.Zupa);

                var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code }, HttpContext.Request.Scheme);

                var outgoingEmail = new OutgoingEmail
                {
                    To = user.Email,
                    From = _transactionalTemplateConfiguration.FromEmail,
                    Template = new Template
                    {
                        Id = _transactionalTemplateConfiguration.ResetPasswordTemplate,
                        Substitutions = new Dictionary<string, string>
                        {
                            {_transactionalTemplateConfiguration.UserNameKey, user.UserName},
                            {_transactionalTemplateConfiguration.LinkKey, callbackUrl}
                        },
                        IsDynamic = false,
                    }
                };

                await _emailSender.SendEmailAsync(outgoingEmail);

                return View(nameof(ForgotPasswordConfirm));
            }

            return View(nameof(ForgotPasswordConfirm));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.InvalidParameter);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var model = new ResetPasswordModel { Email = user.Email, Code = code };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.UserNotFound);
                return BadRequest("A reset link will be sent shortly");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.NewPassword);
            _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Method, Providers.Zupa);

            if (result.Succeeded)
            {
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.Success);
                await _userManager.SetLockoutEndDateAsync(user, null);
                return RedirectToAction("ResetPasswordConfirm");
            }

            if (result.Errors.Any())
            {
                _trackTelemetry.TrackEvent(EventName.ResetPassword, EventType.Action, EventStatus.Fail);

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkAccount(string provider)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = Url.Action(nameof(LinkLoginCallback));
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> LinkAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.LinkAccount, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var currentLogins = await _userManager.GetLoginsAsync(user);

            var model = new LinkAccountsViewModel
            {
                CurrentLogins = currentLogins,
                HasPassword = await _userManager.HasPasswordAsync(user),
                OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                    .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
                    .ToList(),
                ShowRemoveButton = await _userManager.HasPasswordAsync(user) || currentLogins.Count > 1
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.LinkAccount, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                _trackTelemetry.TrackEvent(EventName.LinkAccount, EventType.Action, EventStatus.ExternalLoginInfoNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return RedirectToAction(nameof(LinkAccount));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveLogin, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return RedirectToAction(nameof(LinkAccount));
        }

        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.SetPassword, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(Login));
            }
            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.ChangePassword, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(Login));
            }
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description.Replace("is already taken", "is already in use"));
            }
        }
    }
}
