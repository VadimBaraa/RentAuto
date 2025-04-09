using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RentAutoWeb.Models.ViewModels
{
    public class RegisterStep3ViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, загрузите фото паспорта.")]
        [Display(Name = "Фото паспорта")]
        public IFormFile PassportPhoto { get; set; }

        [Required(ErrorMessage = "Пожалуйста, загрузите фото ВУ.")]
        [Display(Name = "Фото ВУ")]
        public IFormFile DriverLicensePhoto { get; set; }
    }
}

