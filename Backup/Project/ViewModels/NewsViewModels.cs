using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project.Infrastructure;
using Project.Models;

namespace Project.ViewModels
{
    public class NewsIndex
    {
        public PageData<News> News { get; set; }
    }

    public class NewsShow
    {
        public News News { get; set; }
    }
}