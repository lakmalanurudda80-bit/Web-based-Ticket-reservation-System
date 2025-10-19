using EventTicketSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EventTicketSystem.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index()
        {
            var upcomingEvents = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .Where(e => e.EventDate >= DateTime.Now && e.IsActive)
                .OrderBy(e => e.EventDate)
                .Take(6)
                .ToList();

            return View(upcomingEvents);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        // GET: /Home/EventDetails/5
        public ActionResult EventDetails(int id)
        {
            var eventItem = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .FirstOrDefault(e => e.EventId == id && e.IsActive);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            return View(eventItem);
        }

        // GET: /Home/Events
        public ActionResult Events(string search = "", string category = "", string sortBy = "date")
        {
            // Start with base query for active future events
            var eventsQuery = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .Where(e => e.EventDate >= DateTime.Now && e.IsActive);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                eventsQuery = eventsQuery.Where(e =>
                    e.Title.ToLower().Contains(search) ||
                    e.Description.ToLower().Contains(search) ||
                    e.Venue.Name.ToLower().Contains(search) ||
                    e.Category.Name.ToLower().Contains(search));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category) && category != "All Categories")
            {
                eventsQuery = eventsQuery.Where(e => e.Category.Name == category);
            }

            // Apply sorting
            switch (sortBy)
            {
                case "name":
                    eventsQuery = eventsQuery.OrderBy(e => e.Title);
                    break;
                case "price_low":
                    eventsQuery = eventsQuery.OrderBy(e => e.BasePrice);
                    break;
                case "price_high":
                    eventsQuery = eventsQuery.OrderByDescending(e => e.BasePrice);
                    break;
                case "date":
                default:
                    eventsQuery = eventsQuery.OrderBy(e => e.EventDate);
                    break;
            }

            var events = eventsQuery.ToList();

            // Get categories for dropdown
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .Distinct()
                .ToList();

            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SortBy = sortBy;

            return View(events);
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