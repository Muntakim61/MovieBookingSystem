using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.ViewModelsUser
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

    }
}
