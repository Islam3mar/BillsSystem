using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class ItemFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "COMPANY NAME")]
        public int CompanyId { get; set; }

        [Display(Name = "TYPE NAME")]
        public int ItemTypeId { get; set; }

        [Display(Name = "ITEM NAME")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "UNIT NAME")]
        public int UnitId { get; set; }

        [Display(Name = "SELLING PRICE")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "BUYING PRICE")]
        public decimal BuyingPrice { get; set; }

        [Display(Name = "STOCK QUANTITY")]
        public int QuantityInStock { get; set; }

        [Display(Name = "NOTES")]
        public string? Notes { get; set; }


        public byte[]? RowVersion { get; set; }
    }
}
