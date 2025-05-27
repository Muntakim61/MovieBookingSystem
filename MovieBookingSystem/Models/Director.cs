using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class Director
    {
        public int DirectorId { get; set; }

        [Required]
        public string? Name { get; set; } 
        [Required]
        public string? Biography { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? ImageUrl { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();

    }
}
