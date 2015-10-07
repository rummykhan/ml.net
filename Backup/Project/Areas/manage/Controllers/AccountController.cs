using Project.Areas.manage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using Project.Infrastructure;
using System.Web.Security;

namespace Project.Areas.manage.Controllers
{
    public class AccountController : Controller
    {

        public ActionResult Index()
        {
            return RedirectToRoute(new { area = "manage", controller = "Account", action = "Login" });
        }

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("admin"))
                { return RedirectToRoute(new { area = "manage", controller = "Admin", action = "Index" }); }
                else
                {
                    return View(new AdminLoginViewModel());
                }
            }
            else
            {
                return View(new AdminLoginViewModel());
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(AdminLoginViewModel model)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("admin"))
                return RedirectToRoute(new { area = "manage", controller = "Admin", action = "Index" });
            else
            {
                if (!ModelState.IsValid)
                    return View(new AdminLoginViewModel());

                using (ProjectDBEntities Context = new ProjectDBEntities())
                {
                    model.Password = Hashing.CreateHash(model.Password);

                    var admin = Context.Administrators.Where(x => x.AdminUserName == model.Username && x.AdminPassword == model.Password).FirstOrDefault<Administrator>();

                    if (admin != null)
                    {
                        FormsAuthentication.SetAuthCookie(admin.AdminUserName, true);
                        return RedirectToRoute(new { area = "manage", controller = "Admin", action = "Index" });
                    }
                }

                ViewBag.Message = "Incorrect username/password combination..";
                return View(new AdminLoginViewModel());
            }
        }

        public ActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("admin"))
                return RedirectToRoute(new { area = "manage", controller = "Admin", action = "Index" });
            else
            {

                return View();
            }
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult ForgotPassword(AdminForgotPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                using(var Context = new ProjectDBEntities())
                {
                    var admin = Context.Administrators.Where(x => x.AdminEmail == model.Email).FirstOrDefault<Administrator>();
                    if(admin==null)
                    {
                        ViewBag.Message = "No Such User..";
                        return View(model);
                    }

                    string generatedPassword = Hashing.GeneratePassword();
                    if (Mailer.ForgotPasswordAdmin(admin.AdminUserName, generatedPassword))
                    {

                        admin.AdminPassword = Hashing.CreateHash(generatedPassword);
                        Context.SaveChanges();
                        ViewBag.Message = "New Password has been sent to admin email..";
                        return View(new AdminForgotPasswordViewModel());
                    }
                    else
                    {
                        ViewBag.Message = "SMTP is not working.. try later..";
                        return View(new AdminForgotPasswordViewModel());
                    }

                }
            }

            return View(model);
        }
    }
}
