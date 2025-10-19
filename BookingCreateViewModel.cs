using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class BookingCreateViewModel
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        [Range(1, 10)]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Navigation properties for display
        public Event Event { get; set; }
        public Ticket Ticket { get; set; }
    }
}