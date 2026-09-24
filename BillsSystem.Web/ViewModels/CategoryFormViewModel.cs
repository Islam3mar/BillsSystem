using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "COMPANY NAME")]
        [Required(ErrorMessage = "COMPANY NAME is Required")]
        public int CompanyId { get; set; }

        [Display(Name = "TYPE NAME")]
        [Required(ErrorMessage = "TYPE NAME is Required")]
        public int ItemTypeId { get; set; }

        [Display(Name = "CATEGORY NAME")]
        [Required(ErrorMessage = "CATEGORY NAME is Required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }
    }
}
