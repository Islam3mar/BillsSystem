using System.ComponentModel.DataAnnotations;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Web.ViewModels
{
    // القواعد (Required/Range/...) كلها في FluentValidation بس. هنا Display بس.
    public class BillFormViewModel
    {
        // بيتولّد مرة مع فتح الفورم، فلو اتبعت مرتين السيرفر يعرف إنها نفس الفاتورة
        public Guid SubmissionId { get; set; } = Guid.NewGuid();

        [Display(Name = "BILL DATE")]
        [DataType(DataType.Date)]
        public DateTime? BillDate { get; set; }

        [Display(Name = "CLIENT NAME")]
        public int ClientId { get; set; }

        public List<BillItemRowViewModel> Items { get; set; } = new();

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Display(Name = "PERCENTAGE DISCOUNT")]
        public decimal PercentageDiscount { get; set; }

        [Display(Name = "VALUE DISCOUNT")]
        public decimal ValueDiscount { get; set; }

        [Display(Name = "PAID UP")]
        public decimal PaidUp { get; set; }

        [Display(Name = "DUE DATE")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }
}
