using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "COMPANY NAME")]
        public int CompanyId { get; set; }

        [Display(Name = "TYPE NAME")]
        public int ItemTypeId { get; set; }

        [Display(Name = "CATEGORY NAME")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }
    }
}
