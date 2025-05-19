using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AliUsta.Models
{
    public class AlimViewModel
    {
        public int ID { get; set; }

        public int TedarikciID { get; set; }
        public int MalzemeID { get; set; }

        [Required(ErrorMessage = "Miktar girilmesi zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public int Miktar { get; set; }

        [Required(ErrorMessage = "Tarih girilmesi zorunludur.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Tedarikçi Adı")]
        public string TedarikciAdi { get; set; } 

        [Display(Name = "Malzeme Adı")]
        public string MalzemeAdi { get; set; } 

        [Display(Name = "Tedarikçi")]
        public int SelectedTedarikciID { get; set; } 

        public IEnumerable<SelectListItem> Tedarikciler { get; set; } 

        public AlimViewModel()
        {
            Tedarikciler = new List<SelectListItem>();
            Tarih = DateTime.Today;
        }
    }
}