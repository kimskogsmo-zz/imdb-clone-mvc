using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gmbdb.Models
{
    public class ReviewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid CreatorUserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ShortDate
        {
            get { return CreatedDate.ToShortDateString(); }
            set { value = ShortDate; }
        }
        public decimal ReviewRating { get; set; }
        public int UserRating { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string Type { get; set; }
    }
}