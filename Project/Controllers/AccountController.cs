using Project.Infrastructure;
using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("user"))
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            return View(new SignUpViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult SignUp(SignUpViewModel model)
        {

            if (User.Identity.IsAuthenticated && User.IsInRole("user"))
                return RedirectToRoute(new { controller = "Home", action = "Index" });

            if (ModelState.IsValid)
            {
                using (ProjectDBEntities Context = new ProjectDBEntities())
                {
                    var Userx = Context.SiteUsers.Where(ux => ux.UserName == model.Username).FirstOrDefault<SiteUser>();
                    var UserStatus = Context.UserStatus.Where(usx => usx.UserStatusDetail == "Active").FirstOrDefault<UserStatu>();

                    if (Userx != null)
                    {
                        ViewBag.Message = "Username is already taken..";
                        return View(new SignUpViewModel());
                    }

                    var userWithEmail = Context.SiteUsers.Where(x => x.UserEmail == model.UserEmail).FirstOrDefault<SiteUser>();
                    if(userWithEmail!=null)
                    {
                        ViewBag.Message = "There is already a user with same Email..";
                        return View(new SignUpViewModel());
                    }

                    SiteUser SU = new SiteUser()
                    {
                        UserFirstName = model.UserFirstName,
                        UserLastName = model.UserLastName,
                        UserName = model.Username,
                        UserEmail = model.UserEmail,
                        DateCreated = DateTime.Now,
                        UserStatusID = UserStatus.UserStatusID
                    };

                    //Validating Middle Name
                    if (model.UserMiddleName != null)
                        SU.UserMiddleName = model.UserMiddleName;

                    //Hashing Password
                    SU.UserPass = Hashing.CreateHash(model.UserPass);

                    Context.SiteUsers.Add(SU);

                    if (Context.SaveChanges() == 1)
                    {
                        ViewBag.Message = "User Added Successfully";

                    }
                    else
                    {
                        ViewBag.Message = "User Cannot Be Added";

                    }
                    return View(new SignUpViewModel());


                }
            }
            else
                return View(new SignUpViewModel());
        }

        public ActionResult Login()
        {
            
            if (User.Identity.IsAuthenticated && User.IsInRole("user"))
                return RedirectToRoute(new { controller = "Home", action = "Index" });

            Session["lattempts"] = 0;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {

            if (User.Identity.IsAuthenticated && User.IsInRole("user"))
                return RedirectToRoute(new { controller = "Home", action = "Index" });

            int loginAttemts = Convert.ToInt32(Session["lattempts"]);
            loginAttemts++;
            Session["lattempts"] = loginAttemts;
            if (loginAttemts >= 3)
            {
                ViewBag.Message = "You have exceeded max valid login attempts..";
                return View();
            }
            if (ModelState.IsValid)
            {
                using (ProjectDBEntities Context = new ProjectDBEntities())
                {
                    model.Password = Hashing.CreateHash(model.Password);

                    var user = Context.SiteUsers.Where(sxu => sxu.UserName == model.Username && sxu.UserPass == model.Password).ToList();

                    if (user.Count == 1)
                    {
                        if (user[0].UserStatusID != 2)
                        {
                            ViewBag.Message = "ID not yet \"Activated\" by admin..";
                            return View(new LoginViewModel());
                        }

                        FormsAuthentication.SetAuthCookie(model.Username, true);

                        if (!String.IsNullOrWhiteSpace(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToRoute(new { controller = "Home", action = "Index" });
                    }
                    else
                    {
                        ViewBag.Message = "Incorrect username/password combination";
                        return View(new LoginViewModel());
                    }
                }
            }
            else
            {
                return View(new LoginViewModel());
            }
        }

        public ActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToActionPermanent("Index", "Home");
            
            Session["fpattempts"] = 0;

            return View(new ForgotPasswordViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var attempts = Convert.ToInt32(Session["fpattempts"]);
            attempts++;
            Session["fpattempts"] = attempts;

            if (attempts >= 3)
            {
                ModelState.AddModelError("UserName", "You have exceeded the maximum valid attempts..");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            using (var Context = new ProjectDBEntities())
            {
                var user = Context.SiteUsers.Where(sxu => sxu.UserName == model.UserName).FirstOrDefault<SiteUser>();

                if (user == null)
                {
                    ViewBag.Message = "Are you sure about username..??";
                    return View(model);
                }

                if (user != null)
                {
                    var newPassword = Hashing.GeneratePassword();

                    if (Mailer.ForgotPassword(user.UserName, newPassword))
                    {
                        user.UserPass = Hashing.CreateHash(newPassword);
                        Context.SaveChanges();
                        Session["fpattempts"] = null;
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "SMTP Not working.. Try later or Contact Admin..");
                        return View(model);
                    }
                }
            }
            return View();
        }
    }
}
