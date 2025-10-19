using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EventTicketSystem.Models;
using System.Data.Entity;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Organizer
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            if (organizer == null)
            {
                // Create organizer profile if it doesn't exist
                var user = _context.Users.Find(userId);
                organizer = new Organizer
                {
                    UserId = userId,
                    CompanyName = user.FirstName + " " + user.LastName + " Events",
                    ContactEmail = user.Email,
                    ContactPhone = user.PhoneNumber,
                    IsApproved = true,
                    RegistrationDate = DateTime.Now
                };
                _context.Organizers.Add(organizer);
                _context.SaveChanges();
            }

            // Get organizer statistics
            var totalEvents = _context.Events.Count(e => e.OrganizerId == organizer.OrganizerId);
            var activeEvents = _context.Events.Count(e => e.OrganizerId == organizer.OrganizerId && e.IsActive);
            var totalTicketsSold = _context.BookingDetails
                .Count(bd => bd.Ticket.Event.OrganizerId == organizer.OrganizerId && bd.Booking.Status == "Confirmed");

            // Fix: Use nullable decimal and provide default value
            var totalRevenue = _context.BookingDetails
                .Where(bd => bd.Ticket.Event.OrganizerId == organizer.OrganizerId && bd.Booking.Status == "Confirmed")
                .Sum(bd => (decimal?)bd.Quantity * bd.UnitPrice) ?? 0m;

            ViewBag.TotalEvents = totalEvents;
            ViewBag.ActiveEvents = activeEvents;
            ViewBag.TotalTicketsSold = totalTicketsSold;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.Organizer = organizer;

            // Get recent events
            var recentEvents = _context.Events
                .Include("Venue")
                .Include("Category")
                .Where(e => e.OrganizerId == organizer.OrganizerId)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToList();

            return View(recentEvents);
        }

        // GET: /Organizer/Profile
        public ActionResult Profile()
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            if (organizer == null)
            {
                return HttpNotFound();
            }

            return View(organizer);
        }

        // POST: /Organizer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(Organizer model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

                if (organizer == null)
                {
                    return HttpNotFound();
                }

                organizer.CompanyName = model.CompanyName;
                organizer.Description = model.Description;
                organizer.ContactEmail = model.ContactEmail;
                organizer.ContactPhone = model.ContactPhone;
                organizer.Address = model.Address;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // GET: /Organizer/GetOrganizerStats
        public JsonResult GetOrganizerStats()
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            if (organizer == null)
            {
                return Json(new { success = false });
            }

            var totalEvents = _context.Events.Count(e => e.OrganizerId == organizer.OrganizerId);
            var totalTicketsSold = _context.BookingDetails
                .Count(bd => bd.Ticket.Event.OrganizerId == organizer.OrganizerId && bd.Booking.Status == "Confirmed");

            // Fix: Use nullable decimal and provide default value
            var totalRevenue = _context.BookingDetails
                .Where(bd => bd.Ticket.Event.OrganizerId == organizer.OrganizerId && bd.Booking.Status == "Confirmed")
                .Sum(bd => (decimal?)bd.Quantity * bd.UnitPrice) ?? 0m;

            return Json(new
            {
                success = true,
                totalEvents = totalEvents,
                totalTickets = totalTicketsSold,
                totalRevenue = totalRevenue
            }, JsonRequestBehavior.AllowGet);
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