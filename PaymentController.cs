using Stripe;
using System;
using System.Linq;
using System.Web.Mvc;
using EventTicketSystem.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace EventTicketSystem.Controllers
{
    public class PaymentController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        public PaymentController()
        {
            StripeConfiguration.ApiKey = System.Configuration.ConfigurationManager.AppSettings["StripeSecretKey"];
        }

        // GET: /Payment/Create
        [Authorize(Roles = "Customer")]
        public ActionResult Create(int bookingId)
        {
            var userId = User.Identity.GetUserId();
            var booking = _context.Bookings
                .Include(b => b.BookingDetails.Select(bd => bd.Ticket.Event))
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null || booking.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Invalid booking or booking already processed.";
                return RedirectToAction("Index", "Bookings");
            }

            var viewModel = new PaymentViewModel
            {
                BookingId = bookingId,
                Amount = booking.TotalAmount,
                Description = $"Tickets for {booking.BookingDetails.First().Ticket.Event.Title}",
                Currency = "lkr"
            };

            ViewBag.StripePublishableKey = System.Configuration.ConfigurationManager.AppSettings["StripePublishableKey"];
            return View(viewModel);
        }

        // POST: /Payment/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult Process(PaymentViewModel model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var booking = _context.Bookings
                    .Include(b => b.BookingDetails.Select(bd => bd.Ticket))
                    .FirstOrDefault(b => b.BookingId == model.BookingId && b.UserId == userId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index", "Bookings");
                }

                // Create Stripe payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(booking.TotalAmount * 100), // Convert to cents
                    Currency = "lkr",
                    PaymentMethod = model.PaymentMethodId,
                    Confirm = true,
                    ReturnUrl = Url.Action("Success", "Payment", new { bookingId = booking.BookingId }, Request.Url.Scheme),
                    ReceiptEmail = User.Identity.Name,
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "booking_id", booking.BookingId.ToString() },
                        { "user_id", userId }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);

                if (paymentIntent.Status == "succeeded")
                {
                    // Payment successful
                    return ProcessSuccessfulPayment(booking, paymentIntent.Id);
                }
                else if (paymentIntent.Status == "requires_action")
                {
                    // 3D Secure authentication required
                    return Json(new { requires_action = true, payment_intent_client_secret = paymentIntent.ClientSecret });
                }
                else
                {
                    // Payment failed
                    return ProcessFailedPayment(booking, paymentIntent.Status);
                }
            }
            catch (StripeException ex)
            {
                TempData["ErrorMessage"] = $"Payment failed: {ex.Message}";
                return RedirectToAction("Create", new { bookingId = model.BookingId });
            }
        }

        // GET: /Payment/Success
        [Authorize(Roles = "Customer")]
        public ActionResult Success(int bookingId)
        {
            var userId = User.Identity.GetUserId();
            var booking = _context.Bookings
                .Include(b => b.BookingDetails.Select(bd => bd.Ticket.Event))
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null || booking.Status != "Confirmed")
            {
                TempData["ErrorMessage"] = "Booking not found or payment not confirmed.";
                return RedirectToAction("Index", "Bookings");
            }

            return View(booking);
        }

        // GET: /Payment/Cancel
        [Authorize(Roles = "Customer")]
        public ActionResult Cancel(int bookingId)
        {
            var userId = User.Identity.GetUserId();
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking != null && booking.Status == "Pending")
            {
                // Restore ticket quantities
                foreach (var detail in booking.BookingDetails)
                {
                    detail.Ticket.QuantityAvailable += detail.Quantity;
                }

                _context.Bookings.Remove(booking);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            }

            return RedirectToAction("Index", "Bookings");
        }

        private ActionResult ProcessSuccessfulPayment(Booking booking, string paymentIntentId)
        {
            // Update booking status
            booking.Status = "Confirmed";
            booking.PaymentIntentId = paymentIntentId;

            // Update loyalty points
            var user = _context.Users.Find(booking.UserId);
            user.LoyaltyPoints += booking.LoyaltyPointsEarned;

            _context.SaveChanges();

            // Send confirmation email (you can implement this later)
            // SendBookingConfirmationEmail(booking);

            return Json(new { success = true, redirectUrl = Url.Action("Success", new { bookingId = booking.BookingId }) });
        }

        private ActionResult ProcessFailedPayment(Booking booking, string status)
        {
            // Restore ticket quantities
            foreach (var detail in booking.BookingDetails)
            {
                detail.Ticket.QuantityAvailable += detail.Quantity;
            }

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return Json(new
            {
                success = false,
                error = $"Payment failed. Status: {status}"
            });
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