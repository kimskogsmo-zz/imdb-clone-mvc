using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class UserToDelete
    {
        public System.Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<CommentToReview> CommentToReviews { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<UserToReview> UserToReviews { get; set; }
    }
}