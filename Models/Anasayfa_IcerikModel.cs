using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class Anasayfa_IcerikModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen resim yükleyiniz")]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Description { get; set; }

        [StringLength(50, ErrorMessage = "Buton metni en fazla 50 karakter olabilir")]
        public string ButtonText { get; set; }

        [StringLength(200, ErrorMessage = "URL en fazla 200 karakter olabilir")]
        public string ButtonUrl { get; set; }

        [Range(1, 100, ErrorMessage = "Sıralama 1-100 arasında olmalıdır")]
        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        // İçerik türü (Uzman Kadro, Özel Müfredat, Başarı Takibi gibi)
        [Required(ErrorMessage = "İçerik türü zorunludur")]
        [StringLength(50, ErrorMessage = "İçerik türü en fazla 50 karakter olabilir")]
        public string ContentType { get; set; } = "Genel"; // Default value added
        
        // Font Awesome ikon kodu (fa-user-graduate, fa-book-open gibi)
        [StringLength(50, ErrorMessage = "İkon kodu en fazla 50 karakter olabilir")]
        public string IconClass { get; set; }
        
        // Görsel sağda mı gösterilsin?
        public bool IsImageRight { get; set; } = false;
    }
} 