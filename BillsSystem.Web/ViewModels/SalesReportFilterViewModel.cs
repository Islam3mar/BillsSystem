using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class SalesReportFilterViewModel
    {
        [Display(Name = "FROM")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "TO")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }
    }
}
