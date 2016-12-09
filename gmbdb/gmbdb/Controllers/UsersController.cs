using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using gmbdb;
using System.Security.Cryptography;
using System.Text;
using gmbdb.Models;

namespace gmbdb.Controllers
{
    public class UsersController : Controller
    {
        private gmbdbEntities db = new gmbdbEntities();

        // GET: Users
        public ActionResult Index()
        {
            try
            {
                return View(db.Users.ToList());
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }

        // GET: Users/Details/5
        public ActionResult Profile(string username)
        {
            try
            {
                if (username == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                User user = db.Users.FirstOrDefault(r => r.Username == username);

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

        public ActionResult Create()
        {
            return View();
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Username,Password,Salt,Email,FirstName,LastName")] User user)
        {
            if (ModelState.IsValid)
            {
                try { 

                    user.Id = Guid.NewGuid();
                    user.Salt = CreateSalt(6);
                    user.Password = CreateHash(user.Salt, user.Password);

                    if (db.Users.Any(x => x.Email == user.Email))
                    {
                        ModelState.AddModelError("EmailError", "Email adress already taken!");
                        return View();
                    }

                    var util = new RegexUtilities(); //check email with this mf

                    if (!util.IsValidEmail(user.Email))
                    {
                        ModelState.AddModelError("EmailError", "Not valid e-mail!");
                    }
                    if (user.FirstName.Any(char.IsDigit))
                    {
                        ModelState.AddModelError("FirstNameError", "Names can't contain digits!");
                        var newUser = new User();
                        return View(newUser);
                    }
                    if (user.LastName.Any(char.IsDigit))
                    {
                        ModelState.AddModelError("FirstNameError", "Names can't contain digits!");
                        var newUser = new User();
                        return View(newUser);
                    }
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }

                Session["currentUser"] = user;

                return RedirectToAction("Index", "Reviews");
            }

            return View(user);
        }

        public static string CreateSalt(int size)
        {
            //Generate a cryptographic random number
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number
            return Convert.ToBase64String(buff);
        }

        public static string CreateHash(string salt, string pass)
        {
            UnicodeEncoding uEncode = new UnicodeEncoding();

            var saltAndPass = salt + pass;

            //Get the byteArray value of salt+pass concatenated

            byte[] byteArray = uEncode.GetBytes(saltAndPass);
            
            SHA256Managed sha = new SHA256Managed();

            //Create a hash with the byte array from the salt
            byte[] hash = sha.ComputeHash(byteArray);

            //stringify the byte array
            return Convert.ToBase64String(hash);
        }

        public List<Review> ShowUserReviews()
        {
            return db.Reviews.ToList();
        }

        // GET: Users/Edit/5
        
        public ActionResult Logout()
        {
            Session["currentUser"] = null;
            Session.Abandon();
            return RedirectToAction("Login", "Users");
        }

        public ActionResult Login()
        {
            return View();
        }
        

        [HttpPost]
        public ActionResult Login(User incomingUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (db.Users.Any(u => u.Username != incomingUser.Username))
                    {
                        ModelState.AddModelError("NotFound", "User not found");
                    }


                    //get User by provided username, and the salt for that user
                    var user = db.Users.FirstOrDefault(u => u.Username == incomingUser.Username);
                    var salt = user.Salt;

                    //givenPassword is the provided pass from the login form
                    var givenPassword = incomingUser.Password;
                    
                    //new hash from "incomingUser"s password and the real users salt
                    var newHash = CreateHash(salt, givenPassword);

                    if (db.Users.Any(x => x.Username == incomingUser.Username && x.Password == newHash))
                    {
                        Session["currentUser"] = user;
                        FormsAuthentication.SetAuthCookie(user.Username, true);

                        return RedirectToAction("Index", "Reviews");
                    }
                    
                    if (db.Users.Any(u => u.Username == incomingUser.Username))
                    {
                        ModelState.AddModelError("NotFound", "");
                    }

                    ModelState.AddModelError("IncorrectCredentialsError", "Incorrect credentials! Try again!");
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }
            }

            return View();
        }

        public ActionResult EditPassword(string username)
        {
            if (((User) Session["currentUser"]).Username != username)
            {
                Response.Redirect("~/Reviews/Index");
            }

            try
            {
                if (username == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = db.Users.FirstOrDefault(u => u.Username.Equals(username));

                var model = new PasswordModel();

                return View(model);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }
            
            return View();
        }

        [HttpPost]
        public ActionResult EditPassword(PasswordModel newPass)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var username = ((User) Session["currentUser"]).Username;

                    var user = db.Users.FirstOrDefault(x => x.Username == username);

                    var oldPasswordHash = CreateHash(user.Salt, newPass.OldPassword);

                    if (oldPasswordHash != user.Password)
                    {
                        Response.Write("incorrect password!");
                        return View();
                    }

                    var newPasswordHash = CreateHash(user.Salt, newPass.NewPassword);

                    user.Password = newPasswordHash;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }
            }
            return View();
        }

        public ActionResult Edit(Guid id)
        {
            try
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                var model = new EditUserModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username
                };
                
                return View(model);
            }
            catch
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }
        [HttpPost]
        public ActionResult Edit(EditUserModel newUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.Id.Equals(newUser.Id));

                    var users = db.Users;

                    foreach (var checkUser in users)
                    {
                        if (checkUser.Username == newUser.Username && checkUser.Id != newUser.Id)
                        {
                            ModelState.AddModelError("UsernameError", "Username already exists!");

                            return View();
                        }
                    } 

                    if (newUser.FirstName.Any(char.IsDigit))
                    {
                        Response.Write("Names can't contain numbers!");

                        return View();
                    }
                    if (newUser.LastName.Any(char.IsDigit))
                    {
                        Response.Write("Names can't contain numbers!");

                        return View();
                    }

                    user.FirstName = newUser.FirstName;
                    user.LastName = newUser.LastName;
                    user.Username = newUser.Username;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    ((User) Session["currentUser"]).Username = user.Username;
                    
                    Response.Redirect($"~/Users/Profile?username={user.Username}");
                }
                catch
                {
                    RedirectToAction("Error", "Home");
                }
            }
            
            return View();
        }

        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm()
        {
            var userId = ((User) Session["currentUser"]).Id;
            try
            {
                var userToDelete = db.Users.SingleOrDefault(x => x.Id == userId);

                var reviewsToDelete = new List<Review>();
                var commentsToDelete = new List<CommentToReview>();
                var reactionsToDelete = new List<UserToReview>();

                //Add to delete listsssss
                foreach (var review in db.Reviews)
                {
                    if (review.CreatorUserId == userId)
                    {
                        foreach (var comment in review.CommentToReviews)
                        {
                            commentsToDelete.Add(comment);
                        }
                        foreach (var reaction in review.UserToReviews)
                        {
                            reactionsToDelete.Add(reaction);
                        }

                        reviewsToDelete.Add(review);
                    }
                }
                foreach (var comment in db.CommentToReviews)
                {
                    if (comment.UserId == userId) commentsToDelete.Add(comment);
                }
                foreach (var reaction in db.UserToReviews)
                {
                    if (reaction.UserId == userId) reactionsToDelete.Add(reaction);
                }
                //End add to deletelists

                //Actually delete
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
                //End actually delete

                //remove user and finalize
                db.Users.Remove(userToDelete);
                db.SaveChanges();
                Session.Abandon();
                Session["currentUser"] = null;
                return RedirectToAction("Create", "Users");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
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
