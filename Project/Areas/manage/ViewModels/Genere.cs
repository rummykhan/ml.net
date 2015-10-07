using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Project.Models;
using Project.Infrastructure;

namespace Project.Areas.manage.ViewModels
{

    public class GenereIndex 
    {
        public PageData<Genere> Genere { get; set; }
    }

    public class GenerePostForm
    {
        public bool IsNew { get; set; }
        public int? GenereID { get; set; }
        [Required, MaxLength(20)]
        public string Detail { get; set; }

        [Required,Display(Name="Enabled")]
        public bool IsActive { get; set; }
    }
}