using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class BannerModel
    {
        public int Id { get; set; }

        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
        public string Title { get; set; }

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
    }
} 