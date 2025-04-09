using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }
    }
}
