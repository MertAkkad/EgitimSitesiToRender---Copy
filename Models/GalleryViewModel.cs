using System.Collections.Generic;

namespace EgitimSitesi.Models
{
    public class GalleryViewModel
    {
        public List<GalleryModel> Images { get; set; }
        public GalleryType SelectedType { get; set; }
        public string TypeDisplayName { get; set; }
        public List<GalleryTypeInfo> TypeOptions { get; set; }
    }

    public class GalleryTypeInfo
    {
        public GalleryType Type { get; set; }
        public string DisplayName { get; set; }
        public string Url { get; set; }
    }
} 