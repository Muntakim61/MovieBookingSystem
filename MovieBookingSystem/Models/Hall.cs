using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieBookingSystem.Models
{
    public class Hall
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HallId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        public string Location { get; set; } = string.Empty;
    }
}
