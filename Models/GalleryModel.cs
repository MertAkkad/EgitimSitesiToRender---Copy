using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class GalleryModel
    {
        public int Id { get; set; }
        
        [Required]
        public string ImagePath { get; set; }
        
        [Required]
        public GalleryType Type { get; set; }
        
        public string Description { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        public int Order { get; set; }
        
        // Cloudinary public ID for image management
        public string CloudinaryPublicId { get; set; }
    }
    
    public enum GalleryType
    {
        Sınıflarımız,
        [Display(Name = "Çalışma Ve Etüt Odaları")]
        ÇalışmaVeEtütOdaları,
        [Display(Name = "Hobi Alanları")]
        HobiAlanları,
        Aktiviteler,
        Kantin,
        Bahçe,
        Tuvaletler
    }
} 