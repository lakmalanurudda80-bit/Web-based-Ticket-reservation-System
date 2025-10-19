using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class BookingDetail
    {
        public int BookingDetailId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public string QRCodeData { get; set; }

        public bool IsScanned { get; set; } = false;

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }
    }
}