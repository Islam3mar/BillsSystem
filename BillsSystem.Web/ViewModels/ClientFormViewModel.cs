using System.ComponentModel.DataAnnotations;

namespace BillsSystem.Web.ViewModels
{
    public class ClientFormViewModel
    {
        [Display(Name = "NUMBER")]
        public int Id { get; set; }

        [Display(Name = "CLIENT NAME")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "PHONE")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "ADDRESS")]
        public string Address { get; set; } = string.Empty;
    }
}
