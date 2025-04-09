using System.ComponentModel.DataAnnotations;

namespace RentAutoWeb.Models
{
    public class UserAdditionalInfo
    {
        [Key]
        public int Id { get; set; }
        public string PassportData { get; set; }
        public string DriverLicense { get; set; }
        public string PassportPhotoPath { get; set; }
        public string DriverLicensePhotoPath { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
