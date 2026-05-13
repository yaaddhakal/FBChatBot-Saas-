using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.Models.LoginRequest
{
    public class LoginRequestModel
    {
        [Required]
        public string Username { get; set; }= string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }=string.Empty;


    }
}
