using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mimsv2.Models
{
    public class RegisterViewModel
    {
        public LoginModel User { get; set; } = new LoginModel();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Hospitals { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Titles { get; set; } = new List<SelectListItem>();

        public int id { get; set; }

        [Required(ErrorMessage = "Login name is required")]
        public string loginname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("password", ErrorMessage = "Passwords do not match")]
        public string confirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required")]
        public string surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hospital is required")]
        public string hospitalid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        public string titles { get; set; } = string.Empty;
        public string active { get; set; } = string.Empty;
       

        // Additional fields
        public string rm { get; set; } = "local";

        public DateTime dateadd { get; set; } = DateTime.Now;


    }
}
