using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using PagedList.Mvc;
using PagedList;
using System.Web;
using System.Web.Mvc;
using gmbdb;
using gmbdb.Models;

namespace gmbdb.Controllers
{
    public class ReviewsController : Controller
    {
        private gmbdbEntities db = new gmbdbEntities();

        [HttpPost]
        public ActionResult BatchDeleteReviews(Guid[] reviewIds)
        {
            if (reviewIds != null && reviewIds.Length > 0)
            {
                try
                {
                    var reviewsToDelete = new List<Review>();
                    var commentsToDelete = new List<CommentToReview>();
                    var reactionsToDelete = new List<UserToReview>();

                    foreach (Guid t in reviewIds)
                    {
                        foreach (var review in db.Reviews)
                        {
                            if (review.Id == t) reviewsToDelete.Add(review);
                        }
                        foreach (var comment in db.CommentToReviews)
                        {
                            if (comment.ReviewId == t) commentsToDelete.Add(comment);
                        }
                        foreach (var reaction in db.UserToReviews)
                        {
                            if (reaction.ReviewId == t) reactionsToDelete.Add(reaction);
                        }
                    }

                    foreach (var review in reviewsToDelete)
                    {
                        review.CommentToReviews.Clear();
                        review.UserToReviews.Clear();
                        db.Reviews.Remove(review);
                    }
                    foreach (var comment in commentsToDelete)
                    {
                        db.CommentToReviews.Remove(comment);
                    }
                    foreach (var reaction in reactionsToDelete)
                    {
                        db.UserToReviews.Remove(reaction);
                    }

                    db.SaveChanges();
                }
                catch
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            var currUser = ((User) Session["currentUser"]).Username;

            return RedirectToAction("Profile", "Users", new {username = currUser});
        }

        //this ID is a user guid
        public ActionResult DeleteReviews(Guid id)
        {
            try
            {
                var user = db.Users.FirstOrDefault(r => r.Id == id);

                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        } 

        // GET: Reviews
        public ActionResult Index(string sortType, string currentFilter, string search, int? page)
        {
            try
            {
                ViewBag.CurrentFilter = sortType; //use this later

                //allow descending/ascending by just re clicking the column header with these bad boys:
                ViewBag.NameSort = sortType == "Title" ? "Title_Desc" : "Title";
                ViewBag.DateSort = sortType == "Date" ? "Date_Desc" : "Date";
                ViewBag.UserRatingSort = sortType == "UserRating" ? "UserRating_Desc" : "UserRating";
                ViewBag.LikesSort = sortType == "Likes" ? "Likes_Desc" : "Likes";
                ViewBag.DislikesSort = sortType == "Dislikes" ? "Dislikes_Desc" : "Dislikes";
                ViewBag.TypeSort = sortType == "Type" ? "Type_Desc" : "Type";

                
                if (search != null)
                {
                    page = 1;
                }
                else
                {
                    search = currentFilter;
                }

                ViewBag.CurrentFilter = search;

                var reviews = from m in db.Reviews
                    select m;

                if (!string.IsNullOrEmpty(search))
                {
                    reviews = reviews.Where(s => s.Title.Contains(search));
                }

                switch (sortType)
                {
                    case "Title":
                        reviews = reviews.OrderBy(r => r.Title);
                        break;
                    case "Date":
                        reviews = reviews.OrderBy(r => r.CreatedDate);
                        break;
                    case "ReviewRating":
                       reviews = reviews.OrderBy(r => r.ReviewRating);
                        break;
                    case "UserRating":
                        reviews = reviews.OrderBy(r => r.UserRating);
                        break;
                    case "Likes":
                        reviews = reviews.OrderBy(r => r.LikeCount);
                        break;
                    case "Dislikes":
                        reviews = reviews.OrderBy(r => r.DislikeCount);
                        break;
                    case "Type":
                        reviews = reviews.OrderBy(r => r.Type);
                        break;
                    case "Title_Desc":
                        reviews = reviews.OrderByDescending(r => r.Title);
                        break;
                    case "Date_Desc":
                        reviews = reviews.OrderByDescending(r => r.CreatedDate);
                        break;
                    case "ReviewRating_Desc":
                        reviews = reviews.OrderByDescending(r => r.ReviewRating);
                        break;
                    case "UserRating_Desc":
                        reviews = reviews.OrderByDescending(r => r.UserRating);
                        break;
                    case "Likes_Desc":
                        reviews = reviews.OrderByDescending(r => r.LikeCount);
                        break;
                    case "Dislikes_Desc":
                        reviews = reviews.OrderByDescending(r => r.DislikeCount);
                        break;
                    case "Type_Desc":
                        reviews = reviews.OrderByDescending(r => r.Type);
                        break;
                    default:
                        reviews = reviews.OrderBy(r => r.Title);
                        break;
                }

                int pageSize = 7;
                int pageNumber = (page ?? 1);
                return View(reviews.ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }
            return View();
        } //works fine! check!

        public ActionResult RatedBy(Guid id)
        {
            try
            {
                Review review = db.Reviews.FirstOrDefault(r => r.Id == id);

                if (review == null)
                {
                    return HttpNotFound();
                }
                return View(review);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }
        // GET: Reviews/Details/5
        public ActionResult Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Review review = db.Reviews.Find(id);
                if (review == null)
                {
                    return HttpNotFound();
                }
                return View(review);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        } //works fine! check!

        // GET: Reviews/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        } //works fine, kinda! Check!

        
        [HttpPost]
        public JsonResult RateReview(int value, Guid reviewId)
        {
            var status = 0;
            var userId = ((User) Session["currentUser"]).Id;

            try
            {
                //the review passed to this method
                var thisReview = db.Reviews.FirstOrDefault(r => r.Id == reviewId);

                //true if logged in user has rated, liked or disliked this review, false if not
                bool userToThisReview = thisReview.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);

                if (!userToThisReview)
                {
                    var newUserToReview = new UserToReview
                    {
                        Id = Guid.NewGuid(), //new guid
                        UserId = userId, //set logged in userID as Id to new user to review
                        ReviewId = reviewId, //current review id
                        Rating = value, //value is the argument for RateReview(int value .... 
                        HasLiked = false //set false since now we are rating not like/disliking
                    };

                    thisReview.UserToReviews.Add(newUserToReview);

                    status = 1;
                    decimal totalrating = (decimal)CalcScoreLocal(thisReview.Id);

                    db.SaveChanges();

                    return Json(new { status, totalrating });
                }
                else //if there IS a user that has liked/disliked or rated this review, then get that user ->
                {
                    //get the user
                    var existingUserToThisReview = thisReview.UserToReviews.FirstOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                    //set this users rating of this review to the value given
                    existingUserToThisReview.Rating = value;

                    status = 1;
                    decimal totalrating = (decimal)CalcScoreLocal(thisReview.Id);

                    //obvious
                    db.SaveChanges();
                    return Json( new { status, totalrating });
                }
            }
            catch
            {
                status = 2;
                return Json(new { status });
            }
        } //should work!...testing..1..2...3...

        [HttpPost]
        public JsonResult CalcScore(Guid id)
        {
            decimal calculatedRating;

            try
            {
                using (var db = new gmbdbEntities())
                {
                    var review = db.Reviews.SingleOrDefault(u => u.Id == id);

                    int count = 0;

                    foreach (var usertorev in review.UserToReviews)
                    {
                        if (usertorev.Rating > 0)
                        {
                            count++;
                        }
                    }

                    int total = 0;

                    foreach (var usertorev in review.UserToReviews)
                    {
                        if (usertorev.Rating > 0)
                        {
                            total += (int)usertorev.Rating;
                        }
                    }

                    calculatedRating = decimal.Divide(total, count);

                    return Json(calculatedRating);
                }
            }
            catch
            {
                calculatedRating = 0;
                return Json(calculatedRating);
            }
        }

        [HttpPost]
        public decimal CalcScoreLocal(Guid id)
        {
            decimal calculatedRating;

            try
            {
                using (var db = new gmbdbEntities())
                {
                    var review = db.Reviews.SingleOrDefault(u => u.Id == id);

                    int count = 0;

                    foreach (var usertorev in review.UserToReviews)
                    {
                        if (usertorev.Rating > 0)
                        {
                            count++;
                        }
                    }

                    int total = 0;

                    foreach (var usertorev in review.UserToReviews)
                    {
                        if (usertorev.Rating > 0)
                        {
                            total += (int)usertorev.Rating;
                        }
                    }

                    calculatedRating = decimal.Divide(total, count);

                    return calculatedRating;
                }
            }
            catch
            {
                calculatedRating = 0;
                return calculatedRating;
            }
        }

        [HttpPost]
        public JsonResult LikeOrDislike(int val, Guid reviewId)
        {
            var status = 0; //0 is initialization value, 1 = likes, 2 = dislikes, 3 = already liked or disliked!

            var userId = ((User) Session["currentUser"]).Id;

            if (val == 1) //( 1 = like)
            {
                try
                {
                    //the review passed to this method
                    var thisReview = db.Reviews.FirstOrDefault(r => r.Id == reviewId);

                    //true if logged in user has liked/disliked this review, false if not
                    bool userToThisReview = thisReview.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);

                    if (!userToThisReview)
                    {
                        var newUserToReview = new UserToReview
                        {
                            Id = Guid.NewGuid(), //new guid
                            UserId = userId, //set logged in userID as Id to new user to review
                            ReviewId = reviewId, //current review id
                            HasLiked = true //always true if liked or disliked
                        };

                        thisReview.LikeCount++;
                        status = 1;
                        thisReview.UserToReviews.Add(newUserToReview);
                        db.SaveChanges();
                    }
                    else //if there IS a user that has liked/disliked or rated this review, then get that user ->
                    {
                        //get the user
                        var existingUserToThisReview = thisReview.UserToReviews.FirstOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                        //check if the user has liked or disliked
                        if (existingUserToThisReview.HasLiked)
                        {
                            //if has, dont allow to like or dislike
                            status = 3;
                            return Json(status);
                        }

                        //if hasnt, then like this review, and set that users HasLiked to True
                        thisReview.LikeCount++;
                        existingUserToThisReview.HasLiked = true; 
                        status = 1;

                        //obvious
                        db.SaveChanges();
                        return Json(status);
                    }
                }
                catch
                {
                    return Json(status);
                }

                return Json(status);
            }

            else if (val == 2) // val==2 = dislike. --------- this is redundant but it's here for clarity
            {
                try
                {
                    //the review passed to this method
                    var thisReview = db.Reviews.FirstOrDefault(r => r.Id == reviewId);

                    //true if logged in user has liked/disliked this review, false if not
                    bool userToThisReview = thisReview.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);

                    if (!userToThisReview)
                    {
                        var newUserToReview = new UserToReview
                        {
                            Id = Guid.NewGuid(), //new guid
                            UserId = userId, //set logged in userID as Id to new user to review
                            ReviewId = reviewId, //current review id
                            HasLiked = true //always true if liked or disliked
                        };

                        thisReview.DislikeCount++;
                        status = 2;
                        thisReview.UserToReviews.Add(newUserToReview);
                        db.SaveChanges();
                    }
                    else //if there IS a user that has liked/disliked or rated this review, then get that user ->
                    {
                        var existingUserToThisReview = thisReview.UserToReviews.FirstOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                        if (existingUserToThisReview.HasLiked)
                        {
                            status = 3;
                            return Json(status);
                        }
                        else
                        {
                            thisReview.DislikeCount++;
                            status = 2;
                            existingUserToThisReview.HasLiked = true;
                            db.SaveChanges();
                        }
                    }
                }
                catch
                {
                    return Json(status);
                }
            }

            return Json(status);
        } //check!

        public JsonResult PostComment(string text, Guid reviewId)
        {
            var status = 0;

            if (ModelState.IsValid)
            {
                try
                {
                    if (text.Length < 3)
                    {
                        ModelState.AddModelError("CommentError", "Your comment is too short!");
                        status = 2;
                        return Json(status);
                    }

                    //get all reviews
                    var reviews = from m in db.Reviews select m;
                    var users = from u in db.Users select u;

                    var newComment = new CommentToReview();

                    newComment.UserId = ((User)Session["currentUser"]).Id;
                    newComment.Id = Guid.NewGuid();
                    newComment.CreatedDate = DateTime.Now;
                    newComment.ReviewId = reviewId;
                    newComment.Comment = text;
                
                    //loop through all users, and add a user as the newComment.User property only if that user has the same Id as newComment.UserId  **1
                    foreach (var user in users)
                    {
                        if (user.Id == newComment.UserId)
                        {
                            newComment.User = user;
                        }
                    }

                    //loop through all reviews, and add a review as the "newComment.Review" property only if that review has the same ID as newComment.ReviewID **2
                    foreach (var review in reviews)
                    {
                        if (review.Id == newComment.ReviewId)
                        {
                            newComment.Review = review;
                        }
                    }

                    // **1 and **2 had to be done for some reason otherwise Entity thougth that upon assigning "newComment.Review/User" that a new entity should be implicitly created(???!)

                    //find current Review and add this comment to that review
                    foreach (var review in reviews)
                    {
                        if (review.Id == newComment.ReviewId)
                        {
                            review.CommentToReviews.Add(newComment);
                        }
                    }

                    db.SaveChanges();
                    status = 1;
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }
                
                return Json(status);
            }

            return Json(status);
        } //check!

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = 
            "Id,CreatorUserId,Title,Description,CreatedDate,ReviewRating,UserRating,LikeCount,DislikeCount,Type")] Review review)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    review.Id = Guid.NewGuid();
                    review.CreatorUserId = ((User) Session["currentUser"]).Id;
                    review.CreatedDate = DateTime.Today;
                    review.LikeCount = 0;
                    review.ReviewRating = 0;
                    review.DislikeCount = 0;
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
                return View(review);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        } //check!

        public ActionResult Edit(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Review review = db.Reviews.Find(id);

                var model = new EditReviewModel
                {
                    Id = review.Id,
                    Title = review.Title,
                    Description = review.Description,
                    UserRating = review.UserRating,
                    Type = review.Type
                };

                return View(model);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        } //check

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditReviewModel reviewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var review = db.Reviews.FirstOrDefault(x => x.Id == reviewModel.Id);

                    review.Title = reviewModel.Title;
                    review.Description = reviewModel.Description;
                    review.UserRating = reviewModel.UserRating;
                    review.Type = reviewModel.Type;

                    db.Entry(review).State = EntityState.Modified;
                    db.SaveChanges();

                    var username = ((User) Session["currentUser"]).Username;

                    Response.Redirect($"~/Users/Profile?username={username}");

                    return View(review);
                }
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        public ActionResult Delete(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Review review = db.Reviews.Find(id);
                if (review == null)
                {
                    return HttpNotFound();
                }
                return View(review);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                Review review = db.Reviews.Find(id);
                db.Reviews.Remove(review);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

