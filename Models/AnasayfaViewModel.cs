using System.Collections.Generic;

namespace EgitimSitesi.Models
{
    public class AnasayfaViewModel
    {
        public List<BannerModel> Banners { get; set; }
        public List<Anasayfa_IcerikModel> IcerikOgeleri { get; set; }
  
        public List<DuyurularModel> Duyurular { get; set; }

    }
} 