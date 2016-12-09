using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using gmbdb;

namespace gmbdb.Controllers
{
    public class CommentToReviewsController : Controller
    {
        private gmbdbEntities db = new gmbdbEntities();

        // GET: CommentToReviews
        public ActionResult Index()
        {
            try
            {
                var commentToReviews = db.CommentToReviews.Include(c => c.Review).Include(c => c.User);

                return View(commentToReviews.ToList());
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: CommentToReviews/Details/5
        public ActionResult Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var commentToReview = db.CommentToReviews.Find(id);

                if (commentToReview == null)
                {
                    return HttpNotFound();
                }

                return View(commentToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: CommentToReviews/Create
        public ActionResult Create(Guid reviewId)
        {
            try
            {
                var newComment = new CommentToReview();

                newComment.ReviewId = reviewId;

                return PartialView("_CommentPartial", newComment);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: CommentToReviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public JsonResult Create(CommentToReview newComment, Guid reviewId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //get all reviews
                    var reviews = from m in db.Reviews select m;
                    var users = from u in db.Users select u;

                    newComment.UserId = ((User) Session["currentUser"]).Id;
                    newComment.Id = Guid.NewGuid();
                    newComment.CreatedDate = DateTime.Now;

                    if (newComment.Comment.Length < 3)
                    {
                        ModelState.AddModelError("CommentError", "Please actually write a comment!");
                    }

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
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }
                //maybe
                return Json("success");
            }
            
            //rly right?
            return Json("success");
        }

        // GET: CommentToReviews/Edit/5
        public ActionResult Edit(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                CommentToReview commentToReview = db.CommentToReviews.Find(id);

                if (commentToReview == null)
                {
                    return HttpNotFound();
                }

                ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title", commentToReview.ReviewId);
                ViewBag.UserId = new SelectList(db.Users, "Id", "Username", commentToReview.UserId);
                return View(commentToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: CommentToReviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,ReviewId,Comment,CreatedDate")] CommentToReview commentToReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(commentToReview).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title", commentToReview.ReviewId);
                ViewBag.UserId = new SelectList(db.Users, "Id", "Username", commentToReview.UserId);

                return View(commentToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: CommentToReviews/Delete/5
        public ActionResult Delete(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                CommentToReview commentToReview = db.CommentToReviews.Find(id);
                if (commentToReview == null)
                {
                    return HttpNotFound();
                }
                return View(commentToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: CommentToReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                CommentToReview commentToReview = db.CommentToReviews.Find(id);

                db.CommentToReviews.Remove(commentToReview);
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
