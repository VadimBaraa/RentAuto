using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class User : IdentityUser<int>
    {
        [Required(ErrorMessage = "Пожалуйста, введите фамилию.")]
        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя.")]
        [Display(Name = "Имя")]
        public string? FirstName { get; set; }

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        public string? PassportData { get; set; }
        public string? DriverLicense { get; set; }
        public string? PassportPhotoPath { get; set; }
        public string? DriverLicensePhotoPath { get; set; }

    }

}
