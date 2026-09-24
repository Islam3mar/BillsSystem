using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class ClientFormViewModel
    {
        [Display(Name = "NUMBER")]
        public int Id { get; set; }

        [Display(Name = "CLIENT NAME")]
        [Required(ErrorMessage = "CLIENT NAME is Required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "PHONE")]
        [Required(ErrorMessage = "PHONE is Required")]
        [RegularExpression(@"^(010|011|012|015)\d{8}$",
            ErrorMessage = "PHONE must be 11 digits and start with 010, 011, 012 or 015")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "ADDRESS")]
        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; } = string.Empty;
    }
}
