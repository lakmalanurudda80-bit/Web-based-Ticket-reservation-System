using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Event> Events { get; set; }
    }
}