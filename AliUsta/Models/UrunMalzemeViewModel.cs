using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;

namespace AliUsta.Models
{
    public class UrunMalzemeViewModel
    {
        public int ID { get; set; } 

        [Required]
        public int UrunID { get; set; }
        public string UrunAdi { get; set; }

        [Required(ErrorMessage = "Lütfen bir malzeme seçiniz.")]
        [Display(Name = "Malzeme")]
        public int MalzemeID { get; set; }
        public string MalzemeAdi { get; set; }

        [Required(ErrorMessage = "Miktar boş bırakılamaz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int Miktar { get; set; }

        public IEnumerable<SelectListItem> MalzemelerListesi { get; set; }

        public UrunMalzemeViewModel()
        {
            MalzemelerListesi = new List<SelectListItem>();
        }
    }

    public class UrunDetayMalzemeViewModel
    {
        public UrunViewModel Urun { get; set; }
        public List<UrunMalzemeViewModel> UrunMalzemeleri { get; set; }
        public UrunMalzemeViewModel YeniUrunMalzeme { get; set; } 

        public UrunDetayMalzemeViewModel()
        {
            Urun = new UrunViewModel();
            UrunMalzemeleri = new List<UrunMalzemeViewModel>();
            YeniUrunMalzeme = new UrunMalzemeViewModel();
        }
    }
}