using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieBookingSystem.Models
{
    public class MovieEditViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }

        public int SelectedDirectorId { get; set; }
        public List<SelectListItem> Directors { get; set; }

        public List<int> SelectedActorIds { get; set; }
        public List<SelectListItem> Actors { get; set; }
    }
}
