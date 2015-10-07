using Project.Infrastructure;
using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class SongsController : Controller
    {
        private const int PerPage = 20;
        public ActionResult Index(int page = 1)
        {
            if (page == 0)
                page = 1;

            var Context = new ProjectDBEntities();

            var totalTracksCount = Context.Tracks.Count();

            if(User.Identity.IsAuthenticated)
            {
                var Tracks = from t in Context.Tracks
                             join ts in Context.UserTrackShares on t.TrackID equals ts.TrackID
                             where ts.SharingType.SharingTypeDetail == "Public" || ts.SharingType.SharingTypeDetail == "Users Only"
                             select t;

                var currentTrackPage = Tracks
                    .Where(x => x.IsActive == true)
                    .OrderBy(x => x.DateAdded)
                    .OrderByDescending(x => x.TrackGeneres.Count)
                    .Skip((page - 1) * PerPage)
                    .Take(PerPage)
                    .ToList();

                foreach (var item in currentTrackPage)
                {
                    var LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList();
                    item.LikesCount = LikesCount[0];
                }

                return View(new SongsListViewModel
                {
                    Tracks = new PageData<Track>(currentTrackPage, totalTracksCount, page, PerPage)
                });
            }
            else
            {
                var Tracks = from t in Context.Tracks
                             join ts in Context.UserTrackShares on t.TrackID equals ts.TrackID
                             where ts.SharingType.SharingTypeDetail == "Public"
                             select t;

                var currentTrackPage = Tracks
                    .Where(x => x.IsActive == true)
                    .OrderBy(x => x.DateAdded)
                    .OrderByDescending(x => x.TrackGeneres.Count)
                    .Skip((page - 1) * PerPage)
                    .Take(PerPage)
                    .ToList();

                foreach (var item in currentTrackPage)
                {
                    var LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList();
                    item.LikesCount = LikesCount[0];
                }

                return View(new SongsListViewModel
                {
                    Tracks = new PageData<Track>(currentTrackPage, totalTracksCount, page, PerPage)
                });
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SongForm(ShareSongPostForm model)
        {
            model.IsNew = model.ID == null;

            using (var Context = new ProjectDBEntities())
            {

                ViewBag.SharingTypes = Context.SharingTypes.ToList();


                if (!ModelState.IsValid)
                    return View(model);

                var selectedGeneres = model.Generes.Where(x => x.IsChecked).ToList();

                var userGuid = (Context.SiteUsers.Where(ax => ax.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;
                var albumID = (Context.Albums.Where(ax => ax.AlbumTitle == "Default").FirstOrDefault<Album>()).AlbumID;

                if (!Context.SharingTypes.Any(x => x.SharingTypeID == model.SharingTypeID))
                    ModelState.AddModelError("SharingType", "No Such Sharing Exists..");

                if (model.IsNew)
                {
                    if (model.Track == null)
                        ModelState.AddModelError("Track", "Track File is required..");

                    if (model.TrackCover == null)
                        ModelState.AddModelError("TrackCover", "Track Cover is required..");
                    
                    if (!ModelState.IsValid)
                        return View(model);

                    if (!VerifyMP3Extension(model.Track.FileName))
                        ModelState.AddModelError("Track", "Only \".mp3\" file is allowed for Track..");

                    if (!VerifyPictureExtension(model.TrackCover.FileName))
                        ModelState.AddModelError("TrackCover", "Only \".jpg\" & \".png\" files are allowed for cover..");

                    if (!ModelState.IsValid)
                        return View(model);

                    var trackName = Guid.NewGuid() + Path.GetExtension(model.Track.FileName);
                    var trackUploadPath = Server.MapPath("~/users/tracks");
                    var trackPathForDb = "/users/tracks/" + trackName;
                    model.Track.SaveAs(Path.Combine(trackUploadPath, trackName));

                    var trackCoverName = Guid.NewGuid() + Path.GetExtension(model.TrackCover.FileName);
                    var trackCoverUploadPath = Server.MapPath("~/users/tracks/images");
                    model.TrackCover.SaveAs(Path.Combine(trackCoverUploadPath, trackCoverName));
                    var trackCoverPathForDb = "/users/tracks/images/" + trackCoverName;


                    Track track = new Track()
                    {
                        TrackTitle = model.Title,
                        AlbumID = albumID,
                        TrackPath = trackPathForDb,
                        TrackCoverPath = trackCoverPathForDb,
                        DateAdded = DateTime.UtcNow,
                        IsActive = true,
                        UserID = userGuid
                    };
                    
                    Context.Tracks.Add(track);

                    Context.SaveChanges();

                    foreach (var item in selectedGeneres)
                    {
                        Context.TrackGeneres.Add(new TrackGenere
                        {
                            TrackId = track.TrackID,
                            GenereID = (int)item.ID
                        });
                    }

                    Context.SaveChanges();

                    Context.UserTrackShares.Add(new UserTrackShare
                    {
                        TrackID = track.TrackID,
                        SharingTypeID = model.SharingTypeID,
                        SharingDate = DateTime.UtcNow
                    });

                    Context.SaveChanges();
                }
                else
                {
                    var track = Context.Tracks.Where(tx => tx.TrackID == model.ID).FirstOrDefault<Track>();

                    if (track == null)
                        return HttpNotFound();

                    var trackShare = Context.UserTrackShares
                        .Where(x => x.UserTrackSharedID == model.TrackShareID)
                        .FirstOrDefault<UserTrackShare>();

                    if (trackShare == null)
                        return HttpNotFound();

                    string trackPathForDb = null;
                    string oldTrack = null;

                    if (model.Track != null)
                    {
                        if (!VerifyMP3Extension(model.Track.FileName))
                            ModelState.AddModelError("Track", "Only \".mp3\" file is allowed for Track..");
                        
                        if (!ModelState.IsValid)
                            return View(model);

                        oldTrack = track.TrackPath;

                        var trackName = Guid.NewGuid() + Path.GetExtension(model.Track.FileName);
                        var trackUploadPath = Server.MapPath("~/users/tracks");
                        trackPathForDb = "/users/tracks/" + trackName;
                        model.Track.SaveAs(Path.Combine(trackUploadPath, trackName));
                        
                    }
                    else
                        trackPathForDb = track.TrackPath;

                    string trackCoverPathForDb = null;
                    string oldCover = null;
                    if (model.TrackCover != null)
                    {
                        if (!VerifyPictureExtension(model.TrackCover.FileName))
                            ModelState.AddModelError("TrackCover", "Only \".jpg\" & \".png\" files are allowed for cover..");

                        if (!ModelState.IsValid)
                            return View(model);

                        oldCover = track.TrackCoverPath;

                        var trackCoverName = Guid.NewGuid() + Path.GetExtension(model.TrackCover.FileName);
                        var trackCoverUploadPath = Server.MapPath("~/users/tracks/images");
                        model.TrackCover.SaveAs(Path.Combine(trackCoverUploadPath, trackCoverName));
                        trackCoverPathForDb = "/users/tracks/images/" + trackCoverName;
                        RemoveOld(track.TrackCoverPath);
                    }
                    else
                        trackCoverPathForDb = track.TrackCoverPath;

                    if (!ModelState.IsValid)
                        return View(model);
                    
                    track.TrackTitle = model.Title;
                    track.AlbumID = (Guid)model.AlbumID;
                    track.TrackPath = trackPathForDb;
                    track.TrackCoverPath = trackCoverPathForDb;
                    track.DateAdded = DateTime.UtcNow;
                    track.IsActive = model.IsActive;
                    track.UserID = userGuid;

                    trackShare.SharingTypeID = model.SharingTypeID;

                    foreach (var item in Context.TrackGeneres.Where(x => x.TrackId == track.TrackID).ToList())
                    {
                        Context.TrackGeneres.Remove(item);
                    }

                    foreach (var item in selectedGeneres)
                    {
                        Context.TrackGeneres.Add(new TrackGenere
                            {
                                TrackId = track.TrackID,
                                GenereID = (int)item.ID
                            });    
                    }

                    Context.SaveChanges();

                    if (oldTrack != null)
                        RemoveOld(oldTrack);

                    if (oldCover != null)
                        RemoveOld(oldCover);

                }
            }
            return RedirectToAction("ListSongs");
        }

        private void RemoveOld(string path)
        {
            string mappedPath = Server.MapPath("~" + path);
            System.IO.File.Delete(mappedPath);
        }

        [Authorize(Roles = "user")]
        public ActionResult Create()
        {
            var Context = new ProjectDBEntities();

            ViewBag.SharingTypes = Context.SharingTypes.ToList();

            return View("SongForm", new ShareSongPostForm
            {
                IsNew = true,
                Generes = Context.Generes
                .Where(x => x.IsActive == true)
                .Select(genere => new GenereCheckBox
                {
                    ID = genere.GenereID,
                    Name = genere.GenereDetail,
                    IsChecked = false
                }).ToList()
            });
        }

        [Authorize(Roles = "user")]
        private bool VerifyMP3Extension(string fileName)
        {
            if (Path.GetExtension(fileName) == ".mp3")
                return true;
            return false;
        }

        [Authorize(Roles = "user")]
        private bool VerifyPictureExtension(string fileName)
        {
            if (Path.GetExtension(fileName) == ".jpg" || Path.GetExtension(fileName) == ".png")
                return true;
            return false;
        }

        [Authorize(Roles = "user")]
        public ActionResult EditSong(Guid id)
        {
            using (var Context = new ProjectDBEntities())
            {
                var track = Context.Tracks.Find(id);

                if (track == null)
                    return HttpNotFound();

                var trackShare = Context.UserTrackShares.Where(x => x.TrackID == id).FirstOrDefault<UserTrackShare>();

                if (trackShare == null)
                    return HttpNotFound();

                IList<GenereCheckBox> generes = new List<GenereCheckBox>();

                foreach (var item in Context.Generes.Where(g => g.IsActive == true).ToList())
                {
                    var itemIsSelected = Context.TrackGeneres
                        .Where(x => x.GenereID == item.GenereID && x.TrackId == track.TrackID)
                        .FirstOrDefault<TrackGenere>();

                    bool isChecked = false;

                    if (itemIsSelected != null)
                        isChecked = true;

                    generes.Add(new GenereCheckBox
                        {
                            ID = item.GenereID,
                            Name = item.GenereDetail,
                            IsChecked = isChecked
                        });
                }

                ViewBag.SharingTypes = Context.SharingTypes.ToList();

                return View("SongForm", new ShareSongPostForm()
                {
                    ID = track.TrackID,
                    Title = track.TrackTitle,
                    AlbumID = track.AlbumID,
                    TrackPath = track.TrackPath,
                    TrackCoverPath = track.TrackCoverPath,
                    DateAdded = track.DateAdded,
                    IsActive = track.IsActive,
                    UserID = track.SiteUser.UserID,
                    Generes = generes,
                    TrackShareID = trackShare.UserTrackSharedID
                });
            }
        }

        [Authorize(Roles = "user")]
        public ActionResult DeleteSong(Guid trackId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers.Where(sxu => sxu.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

                var track = Context.Tracks.Find(trackId);

                if (track == null)
                    return HttpNotFound();
                else
                {
                    var trackShare = Context.UserTrackShares
                        .Where(x => x.TrackID == track.TrackID)
                        .FirstOrDefault<UserTrackShare>();

                    Context.UserTrackShares.Remove(trackShare);

                    Context.Tracks.Remove(track);
                    Context.SaveChanges();

                    RemoveOld(track.TrackCoverPath);
                    RemoveOld(track.TrackPath);
                }
                return RedirectToAction("ListSongs");
            }
        }

        [Authorize(Roles = "user")]
        public ActionResult TrashSong(Guid trackId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers.Where(sxu => sxu.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

                var track = Context.Tracks.Where(nx => nx.TrackID == trackId && nx.UserID == guid).FirstOrDefault<Track>();

                if (track == null)
                    return HttpNotFound();

                track.IsActive = false;
                Context.SaveChanges();

                return RedirectToAction("ListSongs");
            }
        }

        [Authorize(Roles = "user")]
        public ActionResult RestoreSong(Guid trackId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers.Where(sxu => sxu.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;
                var track = Context.Tracks.Where(nx => nx.TrackID == trackId && nx.UserID == guid).FirstOrDefault<Track>();

                if (track == null)
                    return HttpNotFound();

                track.IsActive = true;
                Context.SaveChanges();

                return RedirectToAction("ListSongs");
            }
        }

        private const int TracksPerPage = 5;
        [Authorize(Roles = "user")]
        public ActionResult ListSongs(int page = 1)
        {
            if (page == 0)
                page = 1;

            var Context = new ProjectDBEntities();

            var userID = (Context.SiteUsers.Where(sux => sux.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

            var totalTracksCount = Context.Tracks.Count();

            var currentTrackPage = Context.Tracks
                .Where(tx => tx.UserID == userID)
                .OrderBy(x => x.DateAdded)
                .Skip((page - 1) * TracksPerPage)
                .Take(TracksPerPage)
                .ToList();

            return View(new SongsListViewModel
            {
                Tracks = new PageData<Track>(currentTrackPage, totalTracksCount, page, TracksPerPage)
            });
        }

        [Authorize(Roles = "user")]
        public ActionResult ListSongsOfAlbum(Guid albumID, int page = 1)
        {


            if (page == 0)
                page = 1;

            var Context = new ProjectDBEntities();

            var album = Context.Albums.Where(x => x.AlbumID == albumID).FirstOrDefault<Album>();

            if (album == null)
                return HttpNotFound();

            ViewBag.TextToDisplay = "Album : ";
            ViewBag.Message = album.AlbumTitle;

            var userID = (Context.SiteUsers.Where(sux => sux.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

            var totalTracksCount = Context.Tracks.Count();

            var currentTrackPage = Context.Tracks
                .Where(tx => tx.UserID == userID && tx.AlbumID == albumID)
                .OrderBy(x => x.DateAdded)
                .Skip((page - 1) * TracksPerPage)
                .Take(TracksPerPage)
                .ToList();

            return View("ListSongs", new SongsListViewModel
            {
                Tracks = new PageData<Track>(currentTrackPage, totalTracksCount, page, TracksPerPage)
            });
        }


        [Authorize(Roles = "user")]
        [HttpPost]
        public ActionResult Like(Guid id)
        {
            var Context = new ProjectDBEntities();

            var userID = (Context.SiteUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

            //check if track exists
            var track = Context.Tracks.Where(x => x.TrackID == id).FirstOrDefault<Track>();

            if (track == null)
                return HttpNotFound();

            //check if track is shared or not and privacy of track is registered user && public..
            var sharedTrack = Context.UserTrackShares
                .Where(x => x.TrackID == id && (x.SharingType.SharingTypeDetail == "Users Only" || x.SharingType.SharingTypeDetail == "Public"))
                .FirstOrDefault<UserTrackShare>();

            if (sharedTrack == null)
                return HttpNotFound();

            //check if track is already liked by the current user
            var liked = Context.UserTrackShareLikes
                .Where(x => x.UserTrackShareID == sharedTrack.UserTrackSharedID && x.LikedBy == userID)
                .FirstOrDefault<UserTrackShareLike>();

            if (liked != null)
            {
                return new JsonResult
                {
                    Data = new { code = "error", message = "You already liked this song" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            }
            else
            {
                Context.UserTrackShareLikes.Add(new UserTrackShareLike
                    {
                        UserTrackShareID = sharedTrack.UserTrackSharedID,
                        LikedBy = userID
                    });

                Context.SaveChanges();

                return new JsonResult
                {
                    Data = new { code = "success", likes = track.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0] },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [Authorize(Roles="user")]
        [HttpGet]
        public ActionResult Download(Guid id)
        {
            var Context = new ProjectDBEntities();

            var track = Context.Tracks.Find(id);

            if (track == null)
                Content("test");

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "track.mp3",

                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());


            var path = Server.MapPath("~"+track.TrackPath);

            byte[] file = System.IO.File.ReadAllBytes(path);

            return File(file, "application/octet-stream");
        }

        public ActionResult Play(Guid id)
        {
            //check if track exist..
            var Context = new ProjectDBEntities();

            if (!Context.Tracks.Any(x => x.TrackID == id))
                return HttpNotFound();

            //check if track is shared..
            if (Context.UserTrackShares.Where(x => x.TrackID == id).FirstOrDefault<UserTrackShare>() == null)
                return HttpNotFound();

            //check if selected track is active..
            if (Context.Tracks.Where(x => x.IsActive == true && x.TrackID == id).FirstOrDefault<Track>() == null)
                return HttpNotFound();

            if (User.Identity.IsAuthenticated)
            {
                //check if track is not private..
                var privateTracks = from t in Context.Tracks
                                    join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                    where uts.SharingType.SharingTypeDetail == "Private" && t.TrackID == id && t.IsActive == true
                                    select t;

                if (privateTracks.ToList().Count > 0)
                    return HttpNotFound();

                var track = Context.Tracks.Where(x => x.TrackID == id).FirstOrDefault<Track>();

                track.LikesCount = track.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];

                return View(track);
            }
            else
            {
                //check if track is not private..
                var privateTracks = from t in Context.Tracks
                                    join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                    where (uts.SharingType.SharingTypeDetail == "Private" || uts.SharingType.SharingTypeDetail == "Users Only")
                                    && t.TrackID == id && t.IsActive == true
                                    select t;

                if (privateTracks.ToList().Count > 0)
                    return HttpNotFound();

                var track = Context.Tracks.Where(x => x.TrackID == id).FirstOrDefault<Track>();

                track.LikesCount = track.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];

                return View(track);
            }
        }
    }
}
