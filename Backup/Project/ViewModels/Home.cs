using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.ViewModels
{
    public class IndexDisplayViewModel
    {
        public IList<Track> Tracks { get; set; }
        public IList<Album> Albums { get; set; }
    }
}