using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using EventTicketSystem.Models;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Admin
        public ActionResult Index()
        {
            // Get dashboard statistics
            var totalUsers = _context.Users.Count();
            var totalEvents = _context.Events.Count();
            var totalBookings = _context.Bookings.Count();
            var totalRevenue = _context.Bookings.Where(b => b.Status == "Confirmed").Sum(b => b.TotalAmount);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

        // GET: /Admin/Users
        public ActionResult Users()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var users = _context.Users.ToList();

            var userViewModels = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                Roles = userManager.GetRoles(user.Id).ToList()
            }).ToList();

            return View(userViewModels);
        }

        // GET: /Admin/Events
        public ActionResult Events()
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.EventDate)
                .ToList();

            return View(events);
        }

        // GET: /Admin/Venues
        public ActionResult Venues()
        {
            var venues = _context.Venues.ToList();
            return View(venues);
        }

        // GET: /Admin/Categories
        public ActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // GET: /Admin/Reports
        public ActionResult Reports()
        {
            // Sales report data
            var salesData = _context.Bookings
                .Where(b => b.Status == "Confirmed")
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                .Select(g => new SalesReportViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToList();

            return View(salesData);
        }

        // GET: /Admin/UserDetails/5
        public ActionResult UserDetails(string id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                Address = user.Address,
                LoyaltyPoints = user.LoyaltyPoints,
                Roles = userManager.GetRoles(user.Id).ToList()
            };

            return View(viewModel);
        }

        // GET: /Admin/EditUser/5
        public ActionResult EditUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            return RedirectToAction("Users");
        }

        // GET: /Admin/ExportUsers
        public ActionResult ExportUsers()
        {
            // In a real application, you would generate Excel/CSV file
            // For now, just redirect back with message
            TempData["SuccessMessage"] = "User export feature will be implemented soon.";
            return RedirectToAction("Users");
        }

        // POST: /Admin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(string name, string description, bool isActive = true)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var category = new Category
                {
                    Name = name,
                    Description = description,
                    IsActive = isActive
                };

                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Category added successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Category name is required.";
            }

            return RedirectToAction("Categories");
        }

        // POST: /Admin/UpdateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCategory(int categoryId, string name, string description, bool isActive)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null)
            {
                category.Name = name;
                category.Description = description;
                category.IsActive = isActive;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Category updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Category not found.";
            }

            return RedirectToAction("Categories");
        }

        // POST: /Admin/ToggleCategory
        [HttpPost]
        public ActionResult ToggleCategory(int categoryId, bool activate)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null)
            {
                category.IsActive = activate;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // POST: /Admin/DeleteCategory
        [HttpPost]
        public ActionResult DeleteCategory(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null && !category.Events.Any())
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Cannot delete category with associated events." });
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