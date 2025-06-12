using System.Formats.Asn1;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mimsv2.Models
{
    public class LoginModel
    {

        public int id  { get; set; }


        [Required(ErrorMessage = "Login name or email is required")]
        public string loginname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string password { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string active { get; set; } = string.Empty;
        public string dateadd { get; set; } = string.Empty;
        public string department { get; set; } = string.Empty;
        public string hospitalid { get; set; } = string.Empty;
        public string titles { get; set; } = string.Empty;
        public string rm { get; set; } = string.Empty;
        public string hospitalname { get; set; } = string.Empty;


    }
}
