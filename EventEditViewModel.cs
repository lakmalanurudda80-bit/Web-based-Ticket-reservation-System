using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class EventEditViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Event description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Event Time")]
        public DateTime EventTime { get; set; }

        [Required(ErrorMessage = "Please select a venue")]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between LKR 0.01 and LKR 100,000")]
        [Display(Name = "Base Price (LKR)")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Total tickets is required")]
        [Range(1, 100000, ErrorMessage = "Total tickets must be between 1 and 100,000")]
        [Display(Name = "Total Tickets Available")]
        public int TotalTickets { get; set; }

        [Display(Name = "Event Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImagePath { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        // Dropdown lists
        public List<Venue> Venues { get; set; }
        public List<Category> Categories { get; set; }
    }
}