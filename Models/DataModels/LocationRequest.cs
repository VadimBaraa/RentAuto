using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class LocationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
