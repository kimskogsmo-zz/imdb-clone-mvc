using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class EditUserModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "You need to enter a username."), StringLength(maximumLength: 50, MinimumLength = 3)]
        public string Username { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You need to enter a first name."), StringLength(maximumLength: 50, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You need to enter a last name."), StringLength(maximumLength: 50, MinimumLength = 2)]
        public string LastName { get; set; }
    }
}