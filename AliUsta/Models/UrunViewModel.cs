using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; 

namespace AliUsta.Models 
{
    public class UrunViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Ürün Adı")]
        public string Adi { get; set; }

        [Required(ErrorMessage = "Fiyat boş bırakılamaz.")]
        [Range(0.01, 100000.00, ErrorMessage = "Fiyat 0.01 ile 100000.00 arasında olmalıdır.")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Fiyat { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Kategori")]
        public int? KategoriID { get; set; }

        [Display(Name = "Kategori Adı")]
        public string KategoriAdi { get; set; } 

        public IEnumerable<SelectListItem> KategorilerListesi { get; set; }

        public List<UrunMalzemeViewModel> UrunMalzemeleri { get; set; }
        public UrunMalzemeViewModel YeniUrunMalzemeFormu { get; set; } 

        public UrunViewModel()
        {
            KategorilerListesi = new List<SelectListItem>();
            UrunMalzemeleri = new List<UrunMalzemeViewModel>(); 
            YeniUrunMalzemeFormu = new UrunMalzemeViewModel();
        }
    }
}