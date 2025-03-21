using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class DuyurularModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur")]
        [Display(Name = "Açıklama")]
        [DataType(DataType.Html)]
        public string Description { get; set; }
        
        // Görsel/resim yolu
        [StringLength(200, ErrorMessage = "Görsel yolu en fazla 200 karakter olabilir")]
        [Display(Name = "Görsel")]
        public string ImagePath { get; set; }

        [StringLength(200)]
        public string CloudinaryPublicId { get; set; }

        // Font Awesome ikon kodu (fa-bullhorn, fa-calendar-alt, fa-trophy gibi)
        [StringLength(50, ErrorMessage = "İkon kodu en fazla 50 karakter olabilir")]
        [Display(Name = "İkon Kodu")]
        public string IconClass { get; set; }

        [StringLength(50, ErrorMessage = "Buton metni en fazla 50 karakter olabilir")]
        [Display(Name = "Buton Metni")]
        public string ButtonText { get; set; } = "Detaylar";

        [StringLength(200, ErrorMessage = "URL en fazla 200 karakter olabilir")]
        [Display(Name = "Buton URL")]
        public string ButtonUrl { get; set; }

        [Range(1, 100, ErrorMessage = "Sıralama 1-100 arasında olmalıdır")]
        [Display(Name = "Sıralama")]
        public int Order { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        // Duyuru türü (Kayıt, Sınav, Başarı Haberi gibi)
        [StringLength(50, ErrorMessage = "Duyuru türü en fazla 50 karakter olabilir")]
        [Display(Name = "Duyuru Türü")]
        public string AnnouncementType { get; set; }
        
        // Detay içeriği - Detaylı duyuru metni (opsiyonel)
        [Display(Name = "Detay İçeriği")]
        [DataType(DataType.Html)]
        public string DetailContent { get; set; }
    }
} 