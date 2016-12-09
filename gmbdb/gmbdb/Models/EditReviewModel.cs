using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class EditReviewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Please enter a title."), StringLength(maximumLength: 50, MinimumLength = 3)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please actually write a review!"), StringLength(maximumLength: 15000, MinimumLength = 10)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please pick a rating.")]
        public int UserRating { get; set; }

        [Required(ErrorMessage = "Please select a type from the list.")]
        public string Type { get; set; }

    }
}