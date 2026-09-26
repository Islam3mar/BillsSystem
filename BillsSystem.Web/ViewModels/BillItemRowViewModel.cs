using BillsSystem.Domain.Enums;

namespace BillsSystem.Web.ViewModels
{
    public class BillItemRowViewModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public DiscountType DiscountType { get; set; } = DiscountType.Value;
        public decimal Discount { get; set; }

        // للعرض بس (مش مصدر الحقيقة)
        public string? ItemName { get; set; }
        public string? UnitName { get; set; }
    }
}
