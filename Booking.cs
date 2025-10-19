using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EventTicketSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled, Completed

        public string PaymentIntentId { get; set; }

        public int LoyaltyPointsEarned { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
    }
}