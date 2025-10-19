using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EventTicketSystem.Models
{
    public class CustomerProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Loyalty Points")]
        public int LoyaltyPoints { get; set; }

        // Add Profile Photo Properties
        [Display(Name = "Profile Photo")]
        public HttpPostedFileBase ProfilePhoto { get; set; }

        public string CurrentProfilePhoto { get; set; }

        // Add statistics properties
        public int UpcomingEventsCount { get; set; }
        public int TotalBookingsCount { get; set; }
    }
}