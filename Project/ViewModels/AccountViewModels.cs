using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Project.ViewModels
{
    public class SignUpViewModel
    {
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public int UserID { get; set; }
        
        [Required]
        [Display(Name="First Name")]
        public string UserFirstName { get; set; }
        
        [Display(Name = "Middle Name")]
        public string UserMiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string UserLastName { get; set; }

        [Required]
        [Display(Name = "Username")]
        [MinLength(5, ErrorMessage = "Username must be at least 5 characters long")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [MinLength(5, ErrorMessage = "The Password must be at least 5 characters long")]
        public string UserPass { get; set; }

        [Required]
        [Display(Name = "Re Password")]
        [DataType(DataType.Password)]
        [Compare("UserPass", ErrorMessage = "Password and Confirm Password Doesn't match")]
        public string UserConfirmPass { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }

        public DateTime DateCreated { get; set; }
        
        public DateTime DateActivated { get; set; }
        
        public int UserStatusID { get; set; }

    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name="Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name="User Name : ")]
        public string UserName { get; set; }
    }
}