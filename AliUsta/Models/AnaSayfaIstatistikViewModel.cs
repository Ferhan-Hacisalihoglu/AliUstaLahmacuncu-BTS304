using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AliUsta.Models
{
    public class AnaSayfaIstatistikViewModel
    {
        public double OrtalamaPuan { get; set; }
        public int ToplamUrunSayisi { get; set; }
        public int ToplamPersonelSayisi { get; set; }
        public int AktifMusteriSayisiSonYil { get; set; }
    }
}