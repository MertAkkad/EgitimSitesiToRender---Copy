using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class KadromuzModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim bilgisi zorunludur")]
        [StringLength(100, ErrorMessage = "İsim bilgisi en fazla 100 karakter olabilir")]
        [Display(Name = "İsim")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Kademe seçimi zorunludur")]
        [Display(Name = "Kademe")]
        public Grade Grade { get; set; }

        [Required(ErrorMessage = "Ünvan/Görev seçimi zorunludur")]
        [Display(Name = "Ünvan/Görev")]
        public StaffTitle Title { get; set; }

        [Required(ErrorMessage = "Alan/Branş bilgisi zorunludur")]
        [StringLength(100, ErrorMessage = "Alan/Branş bilgisi en fazla 100 karakter olabilir")]
        [Display(Name = "Alan/Branş")]
        public string Field { get; set; }

        [StringLength(200, ErrorMessage = "Deneyim bilgisi en fazla 200 karakter olabilir")]
        [Display(Name = "Deneyim")]
        public string? Exp { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [StringLength(100, ErrorMessage = "Email adresi en fazla 100 karakter olabilir")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Sıralama bilgisi zorunludur")]
        [Range(1, 1000, ErrorMessage = "Sıralama 1-1000 arasında olmalıdır")]
        [Display(Name = "Sıralama")]
        public int Order { get; set; } = 1;

        [StringLength(200, ErrorMessage = "Görsel yolu en fazla 200 karakter olabilir")]
        [Display(Name = "Profil Fotoğrafı")]
        public string? ImagePath { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }

    public enum Grade
    {
        [Display(Name = "İlkokul")]
        Ilkokul = 1,
        
        [Display(Name = "Ortaokul")]
        Ortaokul = 2,
        
        [Display(Name = "Lise")]
        Lise = 3,
        
        [Display(Name = "Hepsi")]
        All = 4
    }

    public enum StaffTitle
    {
        [Display(Name = "Yönetici")]
        Yonetici = 1,
        
        [Display(Name = "Öğretmen")]
        Ogretmen = 2
    }
} 