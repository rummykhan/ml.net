using Project.Areas.manage.ViewModels;
using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Project.Areas.manage.Controllers
{
    [Authorize(Roles="admin")]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            var Context = new ProjectDBEntities();
            
            AdminIndexViewModel model = new AdminIndexViewModel();

            model.Users = Context.SiteUsers.ToList();
            model.Tracks = Context.Tracks.ToList();
            model.Albums = Context.Albums.ToList();

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToRoute(new { area = "manage", controller = "Account", action = "Index" });
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(AdminChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(User.Identity.IsAuthenticated && User.IsInRole("admin"))
                {
                    using(var Context = new ProjectDBEntities())
                    {
                        var OldHashedPassword = Hashing.CreateHash(model.OldPassword);

                        var user = Context.Administrators.Where(ax => ax.AdminUserName == User.Identity.Name && ax.AdminPassword == OldHashedPassword).FirstOrDefault<Administrator>();
                        if (user != null)
                        {
                            user.AdminPassword = Hashing.CreateHash(model.NewPassword);
                            Context.SaveChanges();
                            ViewBag.Message = "Password Changed Successfully to " + model.NewPassword;
                            return View(new AdminChangePasswordViewModel());
                        }
                        else
                        {
                            ViewBag.Message = "Old password seems different";
                            return View(new AdminChangePasswordViewModel());
                        }
                    }
                }
            }

            return Content(User.Identity.Name);
        }
    }
}
