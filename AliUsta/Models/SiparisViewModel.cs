using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; 

namespace AliUsta.Models 
{
    public class SiparisViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Lütfen bir müşteri seçiniz.")]
        [Display(Name = "Müşteri")]
        public int MusteriID { get; set; }
        public string MusteriAdiSoyadi { get; set; } 

        [Required(ErrorMessage = "Lütfen bir ürün seçiniz.")]
        [Display(Name = "Ürün")]
        public int UrunID { get; set; }
        public string UrunAdi { get; set; } 


        public int UstaID { get; set; }
        public string UstaAdiSoyadi { get; set; }

        public int KuryeID { get; set; }
        public string KuryeAdiSoyadi { get; set; }

        [Required(ErrorMessage = "Miktar girilmesi zorunludur.")]
        [Range(1, 100, ErrorMessage = "Miktar 1 ile 100 arasında olmalıdır.")]
        public int Miktar { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Toplam Fiyat")]
        public decimal Fiyat { get; set; } 

        [Display(Name = "Sipariş Tarihi")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Lütfen bir adres seçiniz.")]
        [Display(Name = "Teslimat Adresi")]
        public int AdresID { get; set; }
        public string AdresAciklamasi { get; set; } 

        // Dropdown listeleri için
        public IEnumerable<SelectListItem> MusterilerListesi { get; set; }
        public IEnumerable<SelectListItem> UrunlerListesi { get; set; }
        public IEnumerable<SelectListItem> UstalarListesi { get; set; } 
        public IEnumerable<SelectListItem> KuryelerListesi { get; set; }
        public IEnumerable<SelectListItem> AdreslerListesi { get; set; } 

        public SiparisViewModel()
        {
            MusterilerListesi = new List<SelectListItem>();
            UrunlerListesi = new List<SelectListItem>();
            UstalarListesi = new List<SelectListItem>();
            KuryelerListesi = new List<SelectListItem>();
            AdreslerListesi = new List<SelectListItem>();
            Tarih = DateTime.Now;
            Miktar = 1; 
        }
    }
}