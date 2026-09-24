using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class ItemFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "COMPANY NAME")]
        [Required(ErrorMessage = "COMPANY NAME is Required")]
        public int CompanyId { get; set; }

        [Display(Name = "TYPE NAME")]
        [Required(ErrorMessage = "TYPE NAME is Required")]
        public int ItemTypeId { get; set; }

        [Display(Name = "ITEM NAME")]
        [Required(ErrorMessage = "ITEM NAME is Required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "UNIT NAME")]
        [Required(ErrorMessage = "UNIT NAME is Required")]
        public int UnitId { get; set; }

        [Display(Name = "SELLING PRICE")]
        [Required(ErrorMessage = "SELLING PRICE is Required")]
        [Range(0, double.MaxValue, ErrorMessage = "SELLING PRICE Must be Greater than or equal Zero")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "BUYING PRICE")]
        [Required(ErrorMessage = "BUYING PRICE is Required")]
        [Range(0, double.MaxValue, ErrorMessage = "BUYING PRICE Must be Greater than or equal Zero")]
        public decimal BuyingPrice { get; set; }

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }
    }
}
