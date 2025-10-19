using EventTicketSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EventTicketSystem.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;

        public AccountController()
        {
            _context = new ApplicationDbContext();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _context = new ApplicationDbContext();
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string role)
        {
            if (ModelState.IsValid)
            {
                string profilePhotoPath = "/Content/images/user-default.png";

                // Handle profile photo upload
                if (model.ProfilePhoto != null && model.ProfilePhoto.ContentLength > 0)
                {
                    try
                    {
                        profilePhotoPath = SaveProfilePhoto(model.ProfilePhoto);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfilePhoto", "Error uploading profile photo: " + ex.Message);
                        return View(model);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePhoto = profilePhotoPath
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role based on selection
                    if (!string.IsNullOrEmpty(role))
                    {
                        await UserManager.AddToRoleAsync(user.Id, role);
                    }
                    else
                    {
                        await UserManager.AddToRoleAsync(user.Id, "Customer");
                    }

                    // If organizer, create organizer profile
                    if (role == "Organizer")
                    {
                        var organizer = new Organizer
                        {
                            UserId = user.Id,
                            CompanyName = model.CompanyName,
                            ContactEmail = model.Email,
                            ContactPhone = model.PhoneNumber
                        };
                        _context.Organizers.Add(organizer);
                        await _context.SaveChangesAsync();
                    }

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    return RedirectToAction("RedirectToDashboard", "Account");
                }
                AddErrors(result);
            }

            return View(model);
        }

        // Helper method to save profile photo
        private string SaveProfilePhoto(HttpPostedFileBase profilePhoto)
        {
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(profilePhoto.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.");
            }

            // Validate file size (max 5MB)
            if (profilePhoto.ContentLength > 5 * 1024 * 1024)
            {
                throw new Exception("File size too large. Maximum size is 5MB.");
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Server.MapPath("~/Content/uploads/profile-photos/");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            profilePhoto.SaveAs(filePath);

            // Return relative path for database storage
            return "/Content/uploads/profile-photos/" + fileName;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("RedirectToDashboard");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // Redirect based on role after login
        [Authorize]
        public ActionResult RedirectToDashboard()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = userManager.FindByName(User.Identity.Name);
                var roles = userManager.GetRoles(user.Id);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin");
                else if (roles.Contains("Organizer"))
                    return RedirectToAction("Index", "Organizer");
                else if (roles.Contains("Customer"))
                    return RedirectToAction("Index", "Customer");
                else
                    return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login");
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}