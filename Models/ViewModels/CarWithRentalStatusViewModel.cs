
namespace RentAutoWeb.Models
{

    public class CarWithRentalStatusViewModel
    {
        public Car Car { get; set; }
        public Rental Rental { get; set; }

        public bool IsRecommended { get; set; }
        public string RecommendationHint { get; set; }
        public List<MaintenanceRecord> MaintenanceRecords { get; set; }
    }
}