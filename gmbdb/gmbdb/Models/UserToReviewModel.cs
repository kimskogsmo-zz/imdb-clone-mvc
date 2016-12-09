using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class UserToReviewModel
    {
        public bool? HasLiked { get; set; }
        public int? Rating { get; set; }
    }
}