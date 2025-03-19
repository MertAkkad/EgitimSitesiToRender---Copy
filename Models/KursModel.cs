using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class KursModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Kurs tipi gereklidir")]
        [Display(Name = "Kurs Tipi")]
        public string Type { get; set; } // Options: İlkokul, Ortaokul, Lise, LGS, TYT/AYT
        
        [Required(ErrorMessage = "Açıklama gereklidir")]
        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Display(Name = "Detaylar")]
        public string Details { get; set; }
        
        [Display(Name = "Resim")]
        [StringLength(200)]
        public string ImagePath { get; set; }
        
        [Required]
        [Display(Name = "Sıralama")]
        public int Order { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 