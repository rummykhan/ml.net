using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var Context = new ProjectDBEntities();
            
            IndexDisplayViewModel model = new IndexDisplayViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var Tracks = from t in Context.Tracks
                             join ts in Context.UserTrackShares on t.TrackID equals ts.TrackID
                             where ts.SharingType.SharingTypeDetail == "Public" || ts.SharingType.SharingTypeDetail == "Users Only"
                             select t;

                foreach (var item in Tracks)
                {
                    var LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList();
                    item.LikesCount = LikesCount[0];
                }

                model.Tracks = Tracks.ToList();
            }
            else
            {
                var Tracks = from t in Context.Tracks
                             join ts in Context.UserTrackShares on t.TrackID equals ts.TrackID
                             where ts.SharingType.SharingTypeDetail == "Public"
                             select t;

                foreach (var item in Tracks)
                {
                    var LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList();
                    item.LikesCount = LikesCount[0];
                }

                model.Tracks = Tracks.ToList();
            }

            model.Albums = Context.Albums
                .Where(x => x.IsActive == true && x.Tracks.Count > 0 && x.AlbumTitle != "Default")
                .OrderByDescending(x => x.AlbumGeneres.Count)
                .ToList();

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Test(Student data)
        {
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}