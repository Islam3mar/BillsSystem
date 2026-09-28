using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class ItemTypeFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "COMPANY NAME")]
        public int CompanyId { get; set; }

        [Display(Name = "TYPE NAME")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }
    }
}
