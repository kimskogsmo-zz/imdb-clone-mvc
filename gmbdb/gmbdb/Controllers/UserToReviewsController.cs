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
    public class UserToReviewsController : Controller
    {
        private gmbdbEntities db = new gmbdbEntities();

        // GET: UserToReviews
        public ActionResult Index()
        {
            try
            {
                var userToReviews = db.UserToReviews.Include(u => u.Review).Include(u => u.User);
                return View(userToReviews.ToList());
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        public ActionResult Rate(Guid reviewId, int rating)
        {
            return View(); 
        }

        public ActionResult Like(Guid reviewId)
        {
            return View();
        }

        public ActionResult Dislike(Guid reviewId)
        {
            return View();
        }
        // GET: UserToReviews/Details/5
        public ActionResult Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                UserToReview userToReview = db.UserToReviews.Find(id);

                if (userToReview == null)
                {
                    return HttpNotFound();
                }
                return View(userToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: UserToReviews/Create
        public ActionResult Create()
        {
            ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title");
            ViewBag.UserId = new SelectList(db.Users, "Id", "Username");
            return View();
        }

        // POST: UserToReviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,ReviewId,HasLiked,Rating")] UserToReview userToReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    userToReview.Id = Guid.NewGuid();
                    db.UserToReviews.Add(userToReview);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title", userToReview.ReviewId);
                ViewBag.UserId = new SelectList(db.Users, "Id", "Username", userToReview.UserId);
                return View(userToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: UserToReviews/Edit/5
        public ActionResult Edit(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UserToReview userToReview = db.UserToReviews.Find(id);
                if (userToReview == null)
                {
                    return HttpNotFound();
                }
                ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title", userToReview.ReviewId);
                ViewBag.UserId = new SelectList(db.Users, "Id", "Username", userToReview.UserId);
                return View(userToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: UserToReviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,ReviewId,HasLiked,Rating")] UserToReview userToReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(userToReview).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.ReviewId = new SelectList(db.Reviews, "Id", "Title", userToReview.ReviewId);
                ViewBag.UserId = new SelectList(db.Users, "Id", "Username", userToReview.UserId);
                return View(userToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: UserToReviews/Delete/5
        public ActionResult Delete(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UserToReview userToReview = db.UserToReviews.Find(id);
                if (userToReview == null)
                {
                    return HttpNotFound();
                }
                return View(userToReview);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: UserToReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                UserToReview userToReview = db.UserToReviews.Find(id);
                db.UserToReviews.Remove(userToReview);
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
