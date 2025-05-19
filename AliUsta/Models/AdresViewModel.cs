using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AliUsta.Models 
{
    public class AdresViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Adres açıklaması boş bırakılamaz.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Adres Açıklaması")]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Lütfen bir müşteri seçiniz.")]
        [Display(Name = "Müşteri")]
        public int MusteriID { get; set; }

        [Display(Name = "Müşteri")]
        public string MusteriAdiSoyadi { get; set; }

        public IEnumerable<SelectListItem> MusterilerListesi { get; set; }

        public AdresViewModel()
        {
            MusterilerListesi = new List<SelectListItem>();
        }
    }
}