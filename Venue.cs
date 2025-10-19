using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        public int Capacity { get; set; }

        public string Facilities { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Event> Events { get; set; }
    }
}