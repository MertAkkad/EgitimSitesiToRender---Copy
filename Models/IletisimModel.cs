using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class IletisimModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Merkez şube bilgisi zorunludur")]
        [Display(Name = "Merkez Şube")]
        [StringLength(100, ErrorMessage = "Merkez şube bilgisi en fazla 100 karakter olabilir")]
        public string MerkezSube { get; set; }

        [Required(ErrorMessage = "Adres bilgisi zorunludur")]
        [Display(Name = "Adres")]
        [StringLength(500, ErrorMessage = "Adres bilgisi en fazla 500 karakter olabilir")]
        public string Adress { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Display(Name = "Telefon Numarası")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string TelNo { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [Display(Name = "E-posta")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Display(Name = "Google Maps Embed Kodu")]
        [StringLength(1000, ErrorMessage = "Google Maps kodu en fazla 1000 karakter olabilir")]
        public string GoogleMapsEmbed { get; set; }

        [Display(Name = "Çalışma Saatleri")]
        [StringLength(200, ErrorMessage = "Çalışma saatleri en fazla 200 karakter olabilir")]
        public string CalismaSaatleri { get; set; }

        [Display(Name = "Facebook URL")]
        [StringLength(200, ErrorMessage = "Facebook URL en fazla 200 karakter olabilir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string FacebookUrl { get; set; }

        [Display(Name = "Instagram URL")]
        [StringLength(200, ErrorMessage = "Instagram URL en fazla 200 karakter olabilir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string InstagramUrl { get; set; }

        [Display(Name = "YouTube URL")]
        [StringLength(200, ErrorMessage = "YouTube URL en fazla 200 karakter olabilir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string YouTubeUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 