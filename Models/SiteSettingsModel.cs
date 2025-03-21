using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class SiteSettingsModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ActiveLayout { get; set; } = "_Layout"; // Default layout
        
        [Display(Name = "Logo")]
        
        [StringLength(255)]
        public string ImagePath { get; set; } = "/img/download.png"; // Default value
        
        [StringLength(200)]
        public string CloudinaryPublicId { get; set; }
        
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 