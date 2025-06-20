namespace RentAutoWeb.Models
{
    public class CarEditViewModel
    {
        public Car Car { get; set; }

        public List<MaintenanceRecord> MaintenanceRecords { get; set; } = new();

        // При необходимости — поля для добавления нового ТО
        public MaintenanceRecord? NewMaintenanceRecord { get; set; } = new MaintenanceRecord();
    }
}