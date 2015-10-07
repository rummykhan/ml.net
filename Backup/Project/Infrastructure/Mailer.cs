using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project.Models;
using System.Net;
using System.Net.Mail;

namespace Project.Infrastructure
{
    public class Mailer
    {

        public static bool ForgotPassword(string username, string password)
        {
            try
            {
                using (var Context = new ProjectDBEntities())
                {
                    var user = Context.SiteUsers.Where(x => x.UserName == username).FirstOrDefault<SiteUser>();

                    var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(user.UserEmail));
                    message.From = new MailAddress("rehan_manzoor@outlook.com");
                    message.Subject = "Your email subject";
                    message.Body = string.Format(body, "Music Library Official", "rehan_manzoor@outlook.com", "Your new Password is " + password);
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "rehan_manzoor@outlook.com",
                            Password = "00NOKIAcasanova#"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp-mail.outlook.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static bool NotifyNewPassword(string username, string password)
        {
            try
            {
                Guid guid = Guid.Parse(username);
                using (var Context = new ProjectDBEntities())
                {
                    var user = Context.SiteUsers.Where(ux => ux.UserID == guid).FirstOrDefault<SiteUser>();

                    var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(user.UserEmail));
                    message.From = new MailAddress("rehan_manzoor@outlook.com");
                    message.Subject = "Your email subject";
                    message.Body = string.Format(body, "Music Library Official", "rehan_manzoor@outlook.com", "Your new Password is " + password);
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "rehan_manzoor@outlook.com",
                            Password = "00NOKIAcasanova#"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp-mail.outlook.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ForgotPasswordAdmin(string username, string password)
        {
            try
            {
                using (var Context = new ProjectDBEntities())
                {
                    var user = Context.Administrators.Where(x => x.AdminUserName == username).FirstOrDefault<Administrator>();

                    var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(user.AdminEmail));
                    message.From = new MailAddress("rehan_manzoor@outlook.com");
                    message.Subject = "Your email subject";
                    message.Body = string.Format(body, "Music Library Official", "rehan_manzoor@outlook.com", "Your new Password for username : " + user.AdminUserName + " is \"" + password + "\"");
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "rehan_manzoor@outlook.com",
                            Password = "00NOKIAcasanova#"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp-mail.outlook.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}