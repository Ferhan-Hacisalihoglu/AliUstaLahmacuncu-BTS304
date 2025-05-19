using System.ComponentModel.DataAnnotations;

namespace AliUsta.Models
{
    public class MusteriViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Müşteri adı boş bırakılamaz.")]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Müşteri soyadı boş bırakılamaz.")]
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
    }
}