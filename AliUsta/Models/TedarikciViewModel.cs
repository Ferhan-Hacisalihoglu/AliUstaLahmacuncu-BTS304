using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AliUsta.Models
{
    public class TedarikciViewModel
    {
        public int ID { get; set; }
        public string Adi { get; set; }
        public string Telefon { get; set; }
        public string EPosta { get; set; }
        public int? MalzemeID { get; set; }
        public string MalzemeAdi { get; set; }

        public IEnumerable<SelectListItem> MalzemelerListesi { get; set; }

        public TedarikciViewModel()
        {
            MalzemelerListesi = new List<SelectListItem>();
        }
    }
}