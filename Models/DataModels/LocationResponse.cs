using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class LocationResponse
    {
        public string? Address { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
