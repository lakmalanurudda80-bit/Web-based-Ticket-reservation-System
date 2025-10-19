using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EventTicketSystem.Models
{
    public class Organizer
    {
        public int OrganizerId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        public string Description { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public string Address { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}