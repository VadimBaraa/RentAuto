using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class Rental
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; } = null!; 
    public int UserId { get; set; }
    public User? User { get; set; } // Ссылка на пользователя, арендующего авто
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; } // Цена аренды за период
    public RentalStatus Status { get; set; } // Статус аренды (активная, завершена, отменена)

}

public enum RentalStatus
{
    [Display(Name = "Готов к аренде")]
    Ready,

    [Display(Name = "Ожидает оплаты")]
    PendingPayment,

    [Display(Name = "Отклонённый")]
    Rejected,

    [Display(Name = "Активный")]
    Active,

    [Display(Name = "Отменённый")]
    Cancelled
}

}
