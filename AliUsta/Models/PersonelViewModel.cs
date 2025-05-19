using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; 

namespace AliUsta.Models 
{
    public class PersonelViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Personel adı boş bırakılamaz.")]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Personel soyadı boş bırakılamaz.")]
        [StringLength(100)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [StringLength(20)]
        [Display(Name = "Telefon")]
        [DataType(DataType.PhoneNumber)]
        public string Telefon { get; set; }

        [StringLength(100)]
        [Display(Name = "E-Posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string EPosta { get; set; }

        [Required(ErrorMessage = "Maaş boş bırakılamaz.")]
        [Range(0, 1000000.00, ErrorMessage = "Maaş 0 ile 1.000.000,00 arasında olmalıdır.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Maaş")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Maas { get; set; }

        [Display(Name = "Görev")]
        public int? GorevID { get; set; } 

        [Display(Name = "Görev Adı")]
        public string GorevAdi { get; set; } 

        public IEnumerable<SelectListItem> GorevlerListesi { get; set; } 

        public PersonelViewModel()
        {
            GorevlerListesi = new List<SelectListItem>();
        }
    }
}