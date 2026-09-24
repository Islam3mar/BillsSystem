using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class UnitFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "UNIT NAME")]
        [Required(ErrorMessage = "UNIT NAME is Required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }
    }
}
