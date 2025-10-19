using System;
using System.Linq;
using System.Web.Mvc;
using EventTicketSystem.Models;
using System.Data.Entity;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VenuesController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Venues
        public ActionResult Index()
        {
            var venues = _context.Venues.OrderBy(v => v.Name).ToList();
            return View(venues);
        }

        // GET: /Venues/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                venue.IsActive = true;
                _context.Venues.Add(venue);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Venue created successfully!";
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        // GET: /Venues/Edit/5
        public ActionResult Edit(int id)
        {
            var venue = _context.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        // POST: /Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(venue).State = EntityState.Modified;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Venue updated successfully!";
                return RedirectToAction("Index");
            }
            return View(venue);
        }

        // GET: /Venues/Details/5
        public ActionResult Details(int id)
        {
            var venue = _context.Venues
                .Include(v => v.Events)
                .FirstOrDefault(v => v.VenueId == id);

            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        // POST: /Venues/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var venue = _context.Venues.Find(id);
            if (venue != null)
            {
                // Check if venue has events
                if (venue.Events.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete venue that has associated events.";
                }
                else
                {
                    _context.Venues.Remove(venue);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Venue deleted successfully!";
                }
            }
            return RedirectToAction("Index");
        }

        // POST: /Venues/ToggleStatus/5
        [HttpPost]
        public ActionResult ToggleStatus(int id, bool isActive)
        {
            var venue = _context.Venues.Find(id);
            if (venue != null)
            {
                venue.IsActive = isActive;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
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