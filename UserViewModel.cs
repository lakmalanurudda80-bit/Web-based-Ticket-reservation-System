using System;
using System.Collections.Generic;

namespace EventTicketSystem.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public string Address { get; set; }
        public int LoyaltyPoints { get; set; }
        public List<string> Roles { get; set; }
    }
}