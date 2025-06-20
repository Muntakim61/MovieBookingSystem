using Microsoft.AspNetCore.Identity;

namespace MovieBookingSystem.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
