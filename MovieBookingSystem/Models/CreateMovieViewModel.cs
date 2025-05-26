using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MovieBookingSystem.Models
{
    public class CreateMovieViewModel
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        public int Duration { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int SelectedDirectorId { get; set; }

        [Required]
        public List<int> SelectedActorIds { get; set; } = new();

        public List<SelectListItem> Directors { get; set; } = new();

        public List<SelectListItem> Actors { get; set; } = new();
    }
}
