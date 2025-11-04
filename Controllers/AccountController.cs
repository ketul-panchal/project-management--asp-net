using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PMS.Data;
using PMS.Models;
using PMS.ViewModels;

namespace PMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IPasswordHasher<AppUser> _hasher;

        public AccountController(AppDbContext ctx, IPasswordHasher<AppUser> hasher)
        {
            _ctx = ctx;
            _hasher = hasher;
        }

        // ---------- Register ----------
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Message = TempData["msg"];
            return View();

            ViewData["HideFooter"] = true;
             return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            if (await _ctx.AppUsers.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email already registered.");
                return View(model);
            }
            
            var user = new AppUser
            {
                FullName = model.FullName.Trim(),
                Email = email,
                Role = model.Role
            };

            user.PasswordHash = _hasher.HashPassword(user, model.Password);

            _ctx.AppUsers.Add(user);
            await _ctx.SaveChangesAsync();

            TempData["msg"] = "Registration successful. Please sign in.";
            return RedirectToAction(nameof(Login));
        }

        // ---------- Login ----------
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Message = TempData["msg"];
            return View();

            ViewData["HideFooter"] = true;
             return View();
        }
        

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            var user = await _ctx.AppUsers.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || user.Role != model.Role)
            {
                ModelState.AddModelError("", "Invalid email/role or password.");
                return View(model);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email/role or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect to role dashboards
            return user.Role == Role.Admin
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "ProjectManager");
        }

        // ---------- 🔹 Forgot Password ----------
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            var user = await _ctx.AppUsers.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email not registered.");
                return View(model);
            }

            // Instead of email, generate reset link and show on screen
            var token = Guid.NewGuid().ToString(); // simple token
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = user.Email;

            // Redirect to a page that shows reset link
            return RedirectToAction(nameof(DisplayResetLink));
        }

        [HttpGet]
        public IActionResult DisplayResetLink()
        {
            var token = TempData["ResetToken"]?.ToString();
            var email = TempData["ResetEmail"]?.ToString();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(ForgotPassword));

            var resetLink = Url.Action("ResetPassword", "Account", new { token, email }, Request.Scheme);
            ViewBag.ResetLink = resetLink;

            return View();
        }

        // ---------- 🔹 Reset Password ----------
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordVM { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _ctx.AppUsers.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return RedirectToAction(nameof(Login));

            // update password
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            await _ctx.SaveChangesAsync();

            TempData["msg"] = "Password reset successful. Please sign in.";
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public async Task<IActionResult> Logout()
          {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
         }
        public IActionResult AccessDenied() => View();

       

       
    }
}
