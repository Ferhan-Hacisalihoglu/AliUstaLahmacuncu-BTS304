using System.ComponentModel.DataAnnotations;

namespace AliUsta.Models 
{
    public class GorevViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Görev adı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Görev adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Görev Adı")]
        public string Adi { get; set; }
    }
}