using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string? Title { get; set; } 
        [Required]

        public string? Genre { get; set; }
        [Required]

        public DateTime ReleaseDate { get; set; }

        public int Duration { get; set; }
        [Required]
        [Precision(6, 2)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } 
        public int? DirectorId { get; set; }
        public Director? Director { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }
    }
}
