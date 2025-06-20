using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; } 
        public Car? Car { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime MaintenanceDate { get; set; }
        public string? Description { get; set; }
       
    }
}
