using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.Web.Security;
using System.IO;
using Project.Infrastructure;

namespace Project.Controllers
{
    [Authorize(Roles = "user")]
    public class UserController : Controller
    {

        public ActionResult EditProfile()
        {
            using (var Context = new ProjectDBEntities())
            {
                var user = Context.SiteUsers
                    .Where(x => x.UserName == User.Identity.Name)
                    .FirstOrDefault<SiteUser>();

                return View(new EditProfileViewModel
                {
                    ID = user.UserID,
                    FirstName = user.UserFirstName,
                    MiddleName = user.UserMiddleName,
                    LastName = user.UserLastName,
                    Email = user.UserEmail
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult EditProfile(EditProfileViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using (var Context = new ProjectDBEntities())
            {
                var user = Context.SiteUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault<SiteUser>();

                if (user.UserID != model.ID)
                {
                    ViewBag.Message = "You cannot edit others profile..";
                    return View(model);
                }

                var userEmailValidation = Context.SiteUsers
                    .Where(x => x.UserEmail == model.Email && x.UserID != user.UserID)
                    .FirstOrDefault<SiteUser>();

                if (userEmailValidation != null)
                {
                    ViewBag.Message = "There is already a user with same email";
                    return View(model);
                }

                user.UserFirstName = model.FirstName;
                user.UserMiddleName = model.MiddleName;
                user.UserLastName = model.LastName;
                user.UserEmail = model.Email;

                Context.SaveChanges();
                ViewBag.Message = "Profile edited Successfully..";

                return View(model);
            }

        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        public ActionResult Find(string username)
        {
            var Context = new ProjectDBEntities();

            var user = Context.SiteUsers.Where(x => x.UserName == username).FirstOrDefault<SiteUser>();
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var Context = new ProjectDBEntities())
            {
                var oldPassword = Hashing.CreateHash(model.OldPassword);
                var user = Context.SiteUsers
                    .Where(x => x.UserName == User.Identity.Name && x.UserPass == oldPassword)
                    .FirstOrDefault<SiteUser>();

                if (user == null)
                {
                    ViewBag.Message = "Old password doesn't match..";
                    return View(model);
                }

                user.UserPass = Hashing.CreateHash(model.NewPassword);

                Context.SaveChanges();

                ViewBag.Message = "Password Changed Successfully..";
                return View(new ChangePasswordViewModel());
            }
        }
    }
}
