using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Project.Models;

namespace Project.Areas.manage.ViewModels
{
    public class AdminLoginViewModel
    {
        [Display(Name = "Username"), Required(ErrorMessage = "Username is required..")]
        public string Username { get; set; }

        [Display(Name = "Password"), Required(ErrorMessage = "Password is required..")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }

    public class AdminChangePasswordViewModel
    {
        public Guid ID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Re Password")]
        [Compare("NewPassword", ErrorMessage = "Password and Re Password Doesn't match")]
        public string RePassword { get; set; }
    }

    public class AdminIndexViewModel
    {
        public IList<Album> Albums { get; set; }

        public IList<Track> Tracks { get; set; }

        public IList<SiteUser> Users { get; set; }
    }

    public class AdminForgotPasswordViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

}