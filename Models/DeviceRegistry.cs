using System.ComponentModel.DataAnnotations;

namespace AgriStream.Models
{
    public class DeviceRegistry
    {
        [Key]
        [MaxLength(50)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string OperationalStatus { get; set; } = "Inactive";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}