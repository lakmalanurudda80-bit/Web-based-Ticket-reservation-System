using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EventTicketSystem.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? EventId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}