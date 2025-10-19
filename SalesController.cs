using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EventTicketSystem.Models;
using System.Data.Entity;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class SalesController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Sales
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            if (organizer == null)
            {
                TempData["ErrorMessage"] = "Organizer profile not found.";
                return RedirectToAction("Index", "Organizer");
            }

            // Get sales statistics
            var salesData = _context.BookingDetails
                .Where(bd => bd.Ticket.Event.OrganizerId == organizer.OrganizerId &&
                            bd.Booking.Status == "Confirmed")
                .GroupBy(bd => bd.Ticket.EventId)
                .Select(g => new EventSalesViewModel
                {
                    EventId = g.Key,
                    EventTitle = g.FirstOrDefault().Ticket.Event.Title,
                    TotalTicketsSold = g.Sum(bd => bd.Quantity),
                    TotalRevenue = g.Sum(bd => bd.Quantity * bd.UnitPrice),
                    EventDate = g.FirstOrDefault().Ticket.Event.EventDate
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            // Overall statistics
            ViewBag.TotalRevenue = salesData.Sum(s => s.TotalRevenue);
            ViewBag.TotalTicketsSold = salesData.Sum(s => s.TotalTicketsSold);
            ViewBag.AverageTicketPrice = ViewBag.TotalTicketsSold > 0 ?
                ViewBag.TotalRevenue / ViewBag.TotalTicketsSold : 0;

            return View(salesData);
        }

        // GET: /Sales/EventSales/5
        public ActionResult EventSales(int id)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            var eventItem = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .FirstOrDefault(e => e.EventId == id && e.OrganizerId == organizer.OrganizerId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            // Get event sales data
            var salesData = _context.BookingDetails
                .Where(bd => bd.Ticket.EventId == id && bd.Booking.Status == "Confirmed")
                .GroupBy(bd => bd.Ticket.TicketType)
                .Select(g => new TicketTypeSalesViewModel
                {
                    TicketType = g.Key,
                    TicketsSold = g.Sum(bd => bd.Quantity),
                    Revenue = g.Sum(bd => bd.Quantity * bd.UnitPrice),
                    AveragePrice = g.Average(bd => bd.UnitPrice)
                })
                .ToList();

            // Monthly sales data
            var monthlySales = _context.BookingDetails
                .Where(bd => bd.Ticket.EventId == id && bd.Booking.Status == "Confirmed")
                .GroupBy(bd => new { bd.Booking.BookingDate.Year, bd.Booking.BookingDate.Month })
                .Select(g => new MonthlySalesViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TicketsSold = g.Sum(bd => bd.Quantity),
                    Revenue = g.Sum(bd => bd.Quantity * bd.UnitPrice)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            var viewModel = new EventSalesDetailViewModel
            {
                Event = eventItem,
                TicketTypeSales = salesData,
                MonthlySales = monthlySales,
                TotalTicketsSold = salesData.Sum(s => s.TicketsSold),
                TotalRevenue = salesData.Sum(s => s.Revenue)
            };

            return View(viewModel);
        }

        // GET: /Sales/ExportSales
        public ActionResult ExportSales(int? eventId)
        {
            var userId = User.Identity.GetUserId();
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);

            // In a real application, you would generate Excel/PDF
            // For now, just return a message
            TempData["SuccessMessage"] = "Sales export feature will be implemented soon.";

            if (eventId.HasValue)
                return RedirectToAction("EventSales", new { id = eventId });
            else
                return RedirectToAction("Index");
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