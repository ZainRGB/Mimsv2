using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Mimsv2.Models
{
    public class WardModel
    {
        public int id { get; set; }
        public string department { get; set; } = string.Empty;
        public string active { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string hospitalid { get; set; } = string.Empty;
        public string hospitalname { get; set; } = string.Empty;
        public List<SelectListItem> Hospitals { get; set; }
    }
}
