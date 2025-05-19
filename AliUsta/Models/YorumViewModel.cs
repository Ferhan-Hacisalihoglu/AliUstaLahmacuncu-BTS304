using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; 

namespace AliUsta.Models 
{
    public class YorumViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Lütfen bir müşteri seçiniz.")]
        [Display(Name = "Müşteri")]
        public int MusteriID { get; set; }
        public string MusteriAdiSoyadi { get; set; } 

        [Required(ErrorMessage = "Lütfen bir sipariş seçiniz.")]
        [Display(Name = "Sipariş")]
        public int SiparisID { get; set; }
        public string SiparisDetayi { get; set; } 

        [Required(ErrorMessage = "Puan boş bırakılamaz.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Puan { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Yorum Açıklaması")]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Tarih boş bırakılamaz.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)] 
        [Display(Name = "Yorum Tarihi")]
        public DateTime Tarih { get; set; }

        // Dropdown listeleri için
        public IEnumerable<SelectListItem> MusterilerListesi { get; set; }
        public IEnumerable<SelectListItem> SiparislerListesi { get; set; } 

        public YorumViewModel()
        {
            MusterilerListesi = new List<SelectListItem>();
            SiparislerListesi = new List<SelectListItem>();
            Tarih = DateTime.Now; 
            Puan = 5; 
        }
    }
}