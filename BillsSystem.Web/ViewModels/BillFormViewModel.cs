using System.ComponentModel.DataAnnotations;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Web.ViewModels
{
    public class BillFormViewModel
    {
        [Display(Name = "BILL DATE")]
        [Required(ErrorMessage = "BILL DATE is Required")]
        [DataType(DataType.Date)]
        public DateTime? BillDate { get; set; }

        [Display(Name = "CLIENT NAME")]
        [Required(ErrorMessage = "CLIENT NAME is Required")]
        public int ClientId { get; set; }

        public List<BillItemRowViewModel> Items { get; set; } = new();

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Display(Name = "PERCENTAGE DISCOUNT")]
        public decimal PercentageDiscount { get; set; }

        [Display(Name = "VALUE DISCOUNT")]
        public decimal ValueDiscount { get; set; }

        [Display(Name = "PAID UP")]
        [Range(0, double.MaxValue, ErrorMessage = "Paid Up Must be Greater than or equal Zero")]
        public decimal PaidUp { get; set; }
    }
}
