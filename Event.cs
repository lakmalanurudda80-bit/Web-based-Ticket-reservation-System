using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace EventTicketSystem.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime EventTime { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int OrganizerId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        public string ImagePath { get; set; }

        [Required]
        public int TotalTickets { get; set; }

        public int AvailableTickets { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual Organizer Organizer { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}