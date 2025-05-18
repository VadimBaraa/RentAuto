using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models.ViewModels
{
    public class RegisterStep2ViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите паспортные данные.")]
        [Display(Name = "Паспортные данные")]
        public string? PassportData { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите данные ВУ.")]
        [Display(Name = "Данные ВУ")]
        public string? DriverLicense { get; set; }
    }
}
