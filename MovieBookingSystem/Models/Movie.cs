using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]

        public string Genre { get; set; }
        [Required]

        public DateTime ReleaseDate { get; set; }

        public int Duration { get; set; }
        [Required]

        public decimal Price { get; set; }
        [Required]

        
        public int DirectorId { get; set; }
        [Required]
        public Director Director { get; set; }
        [Required]
        public ICollection<MovieActor> MovieActors { get; set; }
    }
}
