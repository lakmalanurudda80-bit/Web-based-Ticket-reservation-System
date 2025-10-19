using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EventTicketSystem.Models;
using System.Data.Entity;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class EventsController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Events
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            if (organizer == null)
            {
                TempData["ErrorMessage"] = "Organizer profile not found.";
                return RedirectToAction("Index", "Organizer");
            }

            var events = _context.Events
                .Include("Venue")
                .Include("Category")
                .Where(e => e.OrganizerId == organizer.OrganizerId)
                .OrderByDescending(e => e.EventDate)
                .ToList();

            return View(events);
        }

        // GET: /Events/Create
        public ActionResult Create()
        {
            var viewModel = new EventCreateViewModel
            {
                Venues = _context.Venues.Where(v => v.IsActive).ToList(),
                Categories = _context.Categories.Where(c => c.IsActive).ToList(),
                EventDate = DateTime.Now.AddDays(7),
                EventTime = DateTime.Now.AddHours(1)
            };

            return View(viewModel);
        }

        // POST: /Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

                if (organizer == null)
                {
                    ModelState.AddModelError("", "Organizer profile not found.");
                    model.Venues = _context.Venues.Where(v => v.IsActive).ToList();
                    model.Categories = _context.Categories.Where(c => c.IsActive).ToList();
                    return View(model);
                }

                var eventItem = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    EventDate = model.EventDate,
                    EventTime = model.EventTime,
                    VenueId = model.VenueId,
                    CategoryId = model.CategoryId,
                    OrganizerId = organizer.OrganizerId,
                    BasePrice = model.BasePrice,
                    TotalTickets = model.TotalTickets,
                    AvailableTickets = model.TotalTickets,
                    ImagePath = model.ImagePath,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Events.Add(eventItem);
                _context.SaveChanges();

                // Create default tickets
                CreateDefaultTickets(eventItem.EventId, model.BasePrice, model.TotalTickets);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("Index");
            }

            // If we got here, something failed; redisplay form
            model.Venues = _context.Venues.Where(v => v.IsActive).ToList();
            model.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: /Events/Edit/5
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events.FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EventEditViewModel
            {
                EventId = eventItem.EventId,
                Title = eventItem.Title,
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                EventTime = eventItem.EventTime,
                VenueId = eventItem.VenueId,
                CategoryId = eventItem.CategoryId,
                BasePrice = eventItem.BasePrice,
                TotalTickets = eventItem.TotalTickets,
                ImagePath = eventItem.ImagePath,
                IsActive = eventItem.IsActive,
                Venues = _context.Venues.Where(v => v.IsActive).ToList(),
                Categories = _context.Categories.Where(c => c.IsActive).ToList()
            };

            return View(viewModel);
        }

        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EventEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
                var eventItem = _context.Events.FirstOrDefault(e => e.EventId == model.EventId && e.OrganizerId == organizer.OrganizerId);

                if (eventItem == null)
                {
                    return HttpNotFound();
                }

                eventItem.Title = model.Title;
                eventItem.Description = model.Description;
                eventItem.EventDate = model.EventDate;
                eventItem.EventTime = model.EventTime;
                eventItem.VenueId = model.VenueId;
                eventItem.CategoryId = model.CategoryId;
                eventItem.BasePrice = model.BasePrice;
                eventItem.IsActive = model.IsActive;
                eventItem.ImagePath = model.ImagePath;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction("Index");
            }

            model.Venues = _context.Venues.Where(v => v.IsActive).ToList();
            model.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(model);
        }

        // GET: /Events/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events
                .Include("Venue")
                .Include("Category")
                .Include("Tickets")
                .FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            return View(eventItem);
        }

        // POST: /Events/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events.FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            // Check if there are any bookings for this event
            var hasBookings = _context.BookingDetails.Any(bd => bd.Ticket.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete event that has existing bookings.";
                return RedirectToAction("Index");
            }

            _context.Events.Remove(eventItem);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Events/GetEventStats
        public JsonResult GetEventStats(int id)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events.FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return Json(new { success = false });
            }

            // Calculate total revenue from bookings
            var totalRevenue = _context.BookingDetails
                .Where(bd => bd.Ticket.EventId == id && bd.Booking.Status == "Confirmed")
                .Sum(bd => bd.Quantity * bd.UnitPrice);

            return Json(new
            {
                success = true,
                availableTickets = eventItem.AvailableTickets,
                soldTickets = eventItem.TotalTickets - eventItem.AvailableTickets,
                totalRevenue = totalRevenue
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: /Events/ToggleEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleEvent(int id, bool activate)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events.FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            eventItem.IsActive = activate;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Event {(activate ? "activated" : "deactivated")} successfully!";
            return RedirectToAction("Details", new { id = id });
        }

        private void CreateDefaultTickets(int eventId, decimal basePrice, int totalTickets)
        {
            var tickets = new[]
            {
                new Ticket
                {
                    EventId = eventId,
                    TicketType = "Regular",
                    Price = basePrice,
                    QuantityAvailable = totalTickets - 20,
                    Description = "Standard admission ticket"
                },
                new Ticket
                {
                    EventId = eventId,
                    TicketType = "VIP",
                    Price = basePrice * 1.5m,
                    QuantityAvailable = 10,
                    Description = "VIP access with premium benefits"
                },
                new Ticket
                {
                    EventId = eventId,
                    TicketType = "Premium",
                    Price = basePrice * 2m,
                    QuantityAvailable = 10,
                    Description = "Premium seating and amenities"
                }
            };

            _context.Tickets.AddRange(tickets);
            _context.SaveChanges();
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