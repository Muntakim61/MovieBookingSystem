using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int Duration { get; set; }

        public decimal Price { get; set; }

        public int DirectorId { get; set; }
        public Director Director { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; }
    }
}
