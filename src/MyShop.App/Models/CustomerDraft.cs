using System;

namespace MyShop.App.Models
{
    public class CustomerDraft
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsMember { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
