using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class BizeYazinModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        [Display(Name = "Ad Soyad")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir")]
        public string Email { get; set; }

        [Display(Name = "Telefon")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        public string TelefonNo { get; set; }

        [Required(ErrorMessage = "Konu alanı zorunludur")]
        [Display(Name = "Konu")]
        [StringLength(100, ErrorMessage = "Konu en fazla 100 karakter olabilir")]
        public string Konu { get; set; }

        [Required(ErrorMessage = "Mesaj alanı zorunludur")]
        [Display(Name = "Mesaj")]
        [StringLength(2000, ErrorMessage = "Mesaj en fazla 2000 karakter olabilir")]
        public string Mesaj { get; set; }

        [Display(Name = "Okundu")]
        public bool Okundu { get; set; } = false;

        [Display(Name = "Gönderim Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [Display(Name = "IP Adresi")]
        [StringLength(50)]
        public string IpAdresi { get; set; }
    }
} 