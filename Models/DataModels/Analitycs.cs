using RentAutoWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AnalyticsReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime ReportDate { get; set; }

    public int TotalRentals { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRevenue { get; set; }

    public int? MostPopularCarId { get; set; }

    [ForeignKey(nameof(MostPopularCarId))]
    public Car? MostPopularCar { get; set; }
}
