using System;
using System.ComponentModel.DataAnnotations;

namespace EgitimSitesi.Models
{
    public class HakkimizdaModel
    {
        public int Id { get; set; }

        [Display(Name = "Görsel")]
        [StringLength(200, ErrorMessage = "Görsel yolu en fazla 200 karakter olabilir")]
        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Tarihçemiz alanı zorunludur")]
        [Display(Name = "Tarihçemiz")]
        public string Tarihcemiz { get; set; }

        [Required(ErrorMessage = "Vizyonumuz alanı zorunludur")]
        [Display(Name = "Vizyonumuz")]
        public string Vizyonumuz { get; set; }

        [Required(ErrorMessage = "Misyonumuz alanı zorunludur")]
        [Display(Name = "Misyonumuz")]
        public string Misyonumuz { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
} 