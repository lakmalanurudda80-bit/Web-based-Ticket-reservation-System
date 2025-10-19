using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class PaymentViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string PaymentMethodId { get; set; }
    }
}