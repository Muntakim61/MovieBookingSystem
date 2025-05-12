using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class Actor
    {
        public int ActorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Biography { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; }
    }
}
