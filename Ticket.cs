using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string TicketType { get; set; } // Regular, VIP, Premium

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }

        public string Description { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
    }
}