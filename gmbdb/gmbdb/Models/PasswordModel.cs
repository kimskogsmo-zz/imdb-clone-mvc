using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class PasswordModel
    {
        [Display(Name = "New Password")]
        [Required(ErrorMessage = "Please enter a password!"), DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Old Password")]
        [Required(ErrorMessage = "Please enter a password!"), DataType(DataType.Password)]
        public string OldPassword { get; set; }
    }
}