using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class SubeModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şube adı zorunludur")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir")]
        [Display(Name = "Şube Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres bilgisi zorunludur")]
        [StringLength(500, ErrorMessage = "Adres bilgisi en fazla 500 karakter olabilir")]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        [Display(Name = "Telefon")]
        public string TelNo { get; set; }

        [Required(ErrorMessage = "Çalışma saatleri zorunludur")]
        [StringLength(100, ErrorMessage = "Çalışma saatleri en fazla 100 karakter olabilir")]
        [Display(Name = "Çalışma Saatleri")]
        public string WorkHours { get; set; }

        // Google Maps Information
        [Required(ErrorMessage = "Enlem bilgisi zorunludur")]
        [Display(Name = "Enlem")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Boylam bilgisi zorunludur")]
        [Display(Name = "Boylam")]
        public double Longitude { get; set; }

        [Display(Name = "Google Harita URL")]
        [StringLength(500, ErrorMessage = "Google Harita URL en fazla 500 karakter olabilir")]
        public string? MapUrl { get; set; }

        [Display(Name = "Zoom Seviyesi")]
        [Range(1, 20, ErrorMessage = "Zoom seviyesi 1-20 arasında olmalıdır")]
        public int ZoomLevel { get; set; } = 15;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Sıralama")]
        public int Order { get; set; } = 1;

        [DataType(DataType.Date)]
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 