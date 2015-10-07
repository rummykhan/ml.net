using Project.Areas.manage.ViewModels;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Project.Infrastructure;

namespace Project.Areas.manage.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        public ActionResult All()
        {
            using (ProjectDBEntities Context = new ProjectDBEntities())
            {
                List<SiteUser> SiteUsers = Context.SiteUsers.ToList();
                List<UsersCreateViewModel> ModelUsers = new List<UsersCreateViewModel>();

                Mapper.CreateMap<SiteUser, UsersCreateViewModel>();

                foreach (var user in SiteUsers)
                {
                    ModelUsers.Add(Mapper.Map<UsersCreateViewModel>(user));
                }

                return View(ModelUsers);
            }
        }

        public ActionResult New()
        {
            return View(new UsersCreateViewModel());
        }

        [HttpPost]
        public ActionResult New(UsersCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var Context = new ProjectDBEntities())
                {

                    var UserStatus = Context.UserStatus.Where(usx => usx.UserStatusDetail == "Pending").FirstOrDefault<UserStatu>();

                    model.DateCreated = DateTime.Now;
                    model.DateActivated = DateTime.Now;

                    Mapper.CreateMap<UsersCreateViewModel, SiteUser>();
                    SiteUser User = Mapper.Map<SiteUser>(model);


                    User.UserStatusID = UserStatus.UserStatusID;
                    User.UserPass = Hashing.CreateHash(model.UserPass);

                    Context.SiteUsers.Add(User);

                    if (Context.SaveChanges() == 1)
                        ViewBag.Message = "User added successfully..";
                    else
                        ViewBag.Message = "User cannot be added at the moment..";
                }
            }
            return View(new UsersCreateViewModel());
        }

        [HttpGet]
        public ActionResult Active(string userId)
        {
            try
            {
                Guid guid = Guid.Parse(userId);

                using (var Context = new ProjectDBEntities())
                {
                    var User = Context.SiteUsers.Where(sxu => sxu.UserID == guid).ToList();

                    if (User.Count > 0)
                    {
                        User[0].UserStatusID = 2;

                        if (Context.SaveChanges() == 1)
                            ViewBag.Message = "User status updated successfully..";
                        else
                            ViewBag.Message = "Unable to update user..";
                    }
                    else
                        ViewBag.Message = "No such user..";

                    return RedirectToAction("All");
                }
            }
            catch(Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult Block(string userId)
        {
            try
            {
                Guid guid = Guid.Parse(userId);
                using (var Context = new ProjectDBEntities())
                {
                    var User = Context.SiteUsers.Where(sxu => sxu.UserID == guid).ToList();

                    if (User.Count > 0)
                    {
                        User[0].UserStatusID = 3;

                        if (Context.SaveChanges() == 1)
                            ViewBag.Message = "User status updated successfully..";
                        else
                            ViewBag.Message = "Unable to update user..";
                    }
                    else
                        ViewBag.Message = "No such user..";

                    return RedirectToAction("All");
                }
            }
            catch(Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string userId)
        {
            try
            {
                Guid guid = Guid.Parse(userId);
                using (var Context = new ProjectDBEntities())
                {
                    var User = Context.SiteUsers.Where(sxu => sxu.UserID == guid).FirstOrDefault<SiteUser>();

                    if (User != null)
                    {
                        UsersResetPasswordViewModel Model = new UsersResetPasswordViewModel();
                        Model.UserID = User.UserID;
                        Model.UserPass = Hashing.GeneratePassword();
                        return View(Model);
                    }
                    else
                        return HttpNotFound();
                }
            }
            catch(Exception)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string userId, UsersResetPasswordViewModel model)
        {
            try
            {
                Guid guid = Guid.Parse(userId);

                using (var Context = new ProjectDBEntities())
                {
                    var User = Context.SiteUsers.Where(sxu => sxu.UserID == guid).FirstOrDefault<SiteUser>();

                    if (User != null)
                    {
                        if (Mailer.NotifyNewPassword(userId, model.UserPass))
                        {
                            User.UserPass = Hashing.CreateHash(model.UserPass);
                            Context.SaveChanges();
                        }
                        else
                        {
                            model.UserID = guid;
                            ModelState.AddModelError("Email", "SMTP Not working..");
                            return View(model);
                        }
                    }
                    else
                        return HttpNotFound();
                }

                return RedirectToAction("All");
            }
            catch(Exception)
            {
                return HttpNotFound();
            }
        }
    }
}