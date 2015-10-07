using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.Areas.manage.ViewModels
{
    public class NewsIndexViewModel
    {
        public PageData<News> News { get; set; }
    }

    public class PostForm
    {
        public bool IsNew { get; set; }

        public int? NewsID { get; set; }

        [Required, MaxLength(128)]
        public string Title { get; set; }

        [Required, MaxLength(200)]
        public string Slug { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Contents { get; set; }
        
        public Guid AdminID { get; set; }

        public DateTime PostedDate { get; set; }
    }
}