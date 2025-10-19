using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EventTicketSystem.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.IO;
using System.Security.Claims;
using Microsoft.Owin.Security;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Customer
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);

            // Get customer statistics
            var upcomingBookings = _context.Bookings
                .Where(b => b.UserId == userId &&
                           b.BookingDetails.Any(bd => bd.Ticket.Event.EventDate >= DateTime.Now))
                .Count();

            var totalBookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .Count();

            ViewBag.UpcomingEvents = upcomingBookings;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.LoyaltyPoints = user?.LoyaltyPoints ?? 0;

            return View();
        }

        // GET: /Customer/Profile
        public ActionResult Profile()
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Calculate statistics
            var upcomingCount = _context.Bookings
                .Count(b => b.UserId == userId &&
                          b.BookingDetails.Any(bd => bd.Ticket.Event.EventDate >= DateTime.Now));

            var totalBookings = _context.Bookings.Count(b => b.UserId == userId);

            var viewModel = new CustomerProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                LoyaltyPoints = user.LoyaltyPoints,
                CurrentProfilePhoto = user.ProfilePhoto,
                UpcomingEventsCount = upcomingCount,
                TotalBookingsCount = totalBookings
            };

            return View(viewModel);
        }

        // POST: /Customer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(CustomerProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Handle profile photo upload
                if (model.ProfilePhoto != null && model.ProfilePhoto.ContentLength > 0)
                {
                    try
                    {
                        user.ProfilePhoto = SaveProfilePhoto(model.ProfilePhoto);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfilePhoto", "Error uploading profile photo: " + ex.Message);
                        return View(model);
                    }
                }

                // Update user profile
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                _context.SaveChanges();

                // Update claims for immediate effect
                UpdateUserClaims(user);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
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

        // Helper method to update user claims
        private void UpdateUserClaims(ApplicationUser user)
        {
            var authenticationManager = HttpContext.GetOwinContext().Authentication;
            var identity = (ClaimsIdentity)User.Identity;

            // Remove existing claims
            var existingFirstNameClaim = identity.FindFirst("FirstName");
            var existingLastNameClaim = identity.FindFirst("LastName");
            var existingPhotoClaim = identity.FindFirst("ProfilePhoto");

            if (existingFirstNameClaim != null) identity.RemoveClaim(existingFirstNameClaim);
            if (existingLastNameClaim != null) identity.RemoveClaim(existingLastNameClaim);
            if (existingPhotoClaim != null) identity.RemoveClaim(existingPhotoClaim);

            // Add updated claims
            identity.AddClaim(new Claim("FirstName", user.FirstName));
            identity.AddClaim(new Claim("LastName", user.LastName));
            identity.AddClaim(new Claim("ProfilePhoto", user.ProfilePhoto ?? "/Content/images/user-default.png"));

            authenticationManager.AuthenticationResponseGrant =
                new AuthenticationResponseGrant(new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });
        }

        // GET: /Customer/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Customer/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindById(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Change password
            var result = userManager.ChangePassword(userId, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        // GET: /Customer/BookingHistory
        public ActionResult BookingHistory()
        {
            var userId = User.Identity.GetUserId();

            var bookings = _context.Bookings
                .Include(b => b.BookingDetails.Select(bd => bd.Ticket.Event))
                .Include(b => b.BookingDetails.Select(bd => bd.Ticket.Event.Venue))
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }

        // GET: /Customer/Loyalty
        public ActionResult Loyalty()
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);

            // Get loyalty points history (you can create a LoyaltyPointsHistory model later)
            ViewBag.LoyaltyPoints = user?.LoyaltyPoints ?? 0;
            ViewBag.NextReward = 1000 - (user?.LoyaltyPoints ?? 0) % 1000;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}