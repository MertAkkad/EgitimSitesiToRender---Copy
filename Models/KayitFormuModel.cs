using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class KayitFormuModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        [Display(Name = "Ad Soyad")]
        [StringLength(100, ErrorMessage = "Ad Soyad alanı en fazla 100 karakter olabilir")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Telefon numarası alanı zorunludur")]
        [Display(Name = "Telefon Numarası")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string TelNo { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        [StringLength(100, ErrorMessage = "E-posta alanı en fazla 100 karakter olabilir")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sınıf alanı zorunludur")]
        [Display(Name = "Sınıf")]
        [StringLength(50, ErrorMessage = "Sınıf alanı en fazla 50 karakter olabilir")]
        public string Grade { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 