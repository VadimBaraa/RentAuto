using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models.ViewModels
{
    public class RegisterStep1ViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите адрес электронной почты.")]
        [EmailAddress(ErrorMessage = "Неверный формат адреса электронной почты.")]
        [Display(Name = "Почта")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию.")]
        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя.")]
        [Display(Name = "Имя")]
        public string? FirstName { get; set; }

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }
        
        [Required(ErrorMessage = "Пожалуйста, введите номер телефона.")]
        [Phone(ErrorMessage = "Неверный формат номера телефона.")]
        [Display(Name = "Номер телефона")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите подтверждение пароля.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Подтверждение пароля")]
        public string? ConfirmPassword { get; set; }

    }
}
