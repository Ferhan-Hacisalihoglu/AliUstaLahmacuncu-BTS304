using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AliUsta.Models
{
    public class MalzemeViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Malzeme adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Malzeme adı en fazla 100 karakter olabilir.")]
        [DisplayName("Malzeme Adı")]
        public string Adi { get; set; }

        public int StokMiktari { get; set; }
    }
}