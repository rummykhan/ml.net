using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        public Guid ID { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [MaxLength(20)]
        public string MiddleName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

    }

    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name="Old Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password are different..")]
        [Display(Name = "Re Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}