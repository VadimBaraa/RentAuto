using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RentAutoWeb.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public int Price { get; set; }
        public string? ImageUrl { get; set; }
        public double Latitude { get; set; }  // Координаты для GPS
        public double Longitude { get; set; }
        public bool IsAvailable { get; set; } // Доступность автомобиля
        double? TargetLatitude { get; set; } // Для машин в пути
        double? TargetLongitude { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }  // Например: Эконом, Бизнес, Премиум
        public string? TransmissionType { get; set; }
        public string? FuelType { get; set; }
        public string? VinNumber { get; set; }
        public string? EngineNumber { get; set; }
        public string? AutoNumber { get; set; }
        public string? HorsePower { get; set; }
        public string? BodyNumber { get; set; }
        public string? Color { get; set; }
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public List<MaintenanceRecord> MaintenanceRecords { get; set; }

    }
}
