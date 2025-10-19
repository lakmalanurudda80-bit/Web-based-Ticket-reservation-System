using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EventTicketSystem.Models;
using System.Data.Entity;

namespace EventTicketSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingsController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: /Bookings
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var bookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }

        // GET: /Bookings/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var booking = _context.Bookings
                .Include("BookingDetails.Ticket.Event.Venue")
                .Include("BookingDetails.Ticket.Event.Category")
                .FirstOrDefault(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }

        // GET: /Bookings/Ticket/5
        public ActionResult Ticket(int id)
        {
            var userId = User.Identity.GetUserId();
            var bookingDetail = _context.BookingDetails
                .Include("Booking.User")
                .Include("Ticket.Event.Venue")
                .Include("Ticket.Event.Category")
                .FirstOrDefault(bd => bd.BookingDetailId == id && bd.Booking.UserId == userId);

            if (bookingDetail == null)
            {
                return HttpNotFound();
            }

            return View(bookingDetail);
        }

        // GET: /Bookings/Create
        public ActionResult Create(int eventId, int ticketId, int quantity)
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            var ticket = _context.Tickets
                .Include("Event.Venue")
                .Include("Event.Category")
                .FirstOrDefault(t => t.TicketId == ticketId && t.EventId == eventId);

            if (ticket == null || ticket.QuantityAvailable < quantity)
            {
                ViewBag.ErrorMessage = "Tickets not available or insufficient quantity.";
                return View("Error");
            }

            var viewModel = new BookingCreateViewModel
            {
                EventId = eventId,
                TicketId = ticketId,
                Event = ticket.Event,
                Ticket = ticket,
                Quantity = quantity,
                TotalAmount = ticket.Price * quantity
            };

            return View(viewModel);
        }

        // POST: /Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = _context.Users.Find(userId);
                var ticket = _context.Tickets.Find(model.TicketId);

                if (ticket == null || ticket.QuantityAvailable < model.Quantity)
                {
                    ModelState.AddModelError("", "Tickets not available or insufficient quantity.");
                    return View(model);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Reserve tickets (reduce availability temporarily)
                        ticket.QuantityAvailable -= model.Quantity;

                        // Create booking with Pending status
                        var booking = new Booking
                        {
                            UserId = userId,
                            BookingDate = DateTime.Now,
                            TotalAmount = ticket.Price * model.Quantity,
                            Status = "Pending", // Changed from "Confirmed" to "Pending"
                            LoyaltyPointsEarned = (int)(ticket.Price * model.Quantity / 100)
                        };

                        _context.Bookings.Add(booking);
                        _context.SaveChanges();

                        // Create booking detail
                        var bookingDetail = new BookingDetail
                        {
                            BookingId = booking.BookingId,
                            TicketId = model.TicketId,
                            Quantity = model.Quantity,
                            UnitPrice = ticket.Price,
                            QRCodeData = $"TEMP:{booking.BookingId}:{Guid.NewGuid()}"
                        };

                        _context.BookingDetails.Add(bookingDetail);
                        _context.SaveChanges();

                        transaction.Commit();

                        // Redirect to payment instead of confirming directly
                        return RedirectToAction("Create", "Payment", new { bookingId = booking.BookingId });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "An error occurred while processing your booking. Please try again.");
                        // Log the exception
                    }
                }
            }

            // If we got this far, something failed; redisplay form
            if (model.EventId > 0)
            {
                model.Event = _context.Events
                    .Include("Venue")
                    .Include("Category")
                    .FirstOrDefault(e => e.EventId == model.EventId);
            }
            if (model.TicketId > 0)
            {
                model.Ticket = _context.Tickets.Find(model.TicketId);
            }

            return View(model);
        }

        // Alternative simpler approach - use this if the above doesn't work:
        private ActionResult CreateBookingSimple(BookingCreateViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            var ticket = _context.Tickets.Find(model.TicketId);

            if (ticket == null || ticket.QuantityAvailable < model.Quantity)
            {
                ModelState.AddModelError("", "Tickets not available or insufficient quantity.");
                return View(model);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Reserve tickets temporarily
                    ticket.QuantityAvailable -= model.Quantity;

                    // Create booking with Pending status
                    var booking = new Booking
                    {
                        UserId = userId,
                        BookingDate = DateTime.Now,
                        TotalAmount = ticket.Price * model.Quantity,
                        Status = "Pending",
                        LoyaltyPointsEarned = (int)(ticket.Price * model.Quantity / 100)
                    };

                    _context.Bookings.Add(booking);
                    _context.SaveChanges();

                    // Create booking detail with GUID only for now
                    var bookingDetail = new BookingDetail
                    {
                        BookingId = booking.BookingId,
                        TicketId = model.TicketId,
                        Quantity = model.Quantity,
                        UnitPrice = ticket.Price,
                        QRCodeData = $"TEMP:{Guid.NewGuid()}" // Simple approach
                    };

                    _context.BookingDetails.Add(bookingDetail);
                    _context.SaveChanges();
                    transaction.Commit();

                    // Redirect to payment
                    return RedirectToAction("Create", "Payment", new { bookingId = booking.BookingId });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "An error occurred while processing your booking. Please try again.");
                }
            }

            return View(model);
        }

        // POST: /Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var userId = User.Identity.GetUserId();
            var booking = _context.Bookings
                .Include("BookingDetails.Ticket")
                .FirstOrDefault(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            // Check if cancellation is allowed (e.g., at least 24 hours before event)
            var earliestEventDate = booking.BookingDetails.Min(bd => bd.Ticket.Event.EventDate);
            if (earliestEventDate <= DateTime.Now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Cancellation is not allowed within 24 hours of the event.";
                return RedirectToAction("Details", new { id = id });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Restore ticket quantities
                    foreach (var detail in booking.BookingDetails)
                    {
                        detail.Ticket.QuantityAvailable += detail.Quantity;
                    }

                    // Remove loyalty points
                    var user = _context.Users.Find(userId);
                    user.LoyaltyPoints -= booking.LoyaltyPointsEarned;

                    // Update booking status
                    booking.Status = "Cancelled";

                    _context.SaveChanges();
                    transaction.Commit();

                    TempData["SuccessMessage"] = "Booking cancelled successfully.";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "An error occurred while cancelling the booking.";
                }
            }

            return RedirectToAction("Index");
        }

        // Add this method for downloading tickets
        public ActionResult DownloadTicket(int id)
        {
            var userId = User.Identity.GetUserId();
            var bookingDetail = _context.BookingDetails
                .Include("Booking.User")
                .Include("Ticket.Event.Venue")
                .Include("Ticket.Event.Category")
                .FirstOrDefault(bd => bd.BookingDetailId == id && bd.Booking.UserId == userId);

            if (bookingDetail == null)
            {
                return HttpNotFound();
            }

            // Return PDF or other format for download
            // For now, redirect to ticket view
            return RedirectToAction("Ticket", new { id = id });
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