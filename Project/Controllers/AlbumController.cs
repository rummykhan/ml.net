using Project.Infrastructure;
using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class AlbumController : Controller
    {
        private const int PerPage = 20;
        public ActionResult Index(int page = 1)
        {
            if (page == 0)
                page = 1;

            var Context = new ProjectDBEntities();

            var totalAlbumsCount = Context.Albums.Count();

            var currentAlbumPage = Context.Albums
                .Where(x => x.IsActive == true && x.AlbumTitle != "Default" && x.Tracks.Count > 0)
                .OrderBy(x => x.DateAdded)
                .Skip((page - 1) * PerPage)
                .Take(PerPage)
                .ToList();

            return View(new AlbumsListViewModel
            {
                Albums = new PageData<Album>(currentAlbumPage, totalAlbumsCount, page, PerPage)
            });
        }

        [Authorize(Roles = "user")]
        public ActionResult CreateAlbum()
        {
            var Context = new ProjectDBEntities();

            return View("AlbumForm", new CreateAlbumPostForm
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
        public ActionResult EditAlbum(Guid id)
        {
            using (var Context = new ProjectDBEntities())
            {
                var album = Context.Albums.Where(tx => tx.AlbumID == id).FirstOrDefault<Album>();

                if (album == null)
                    return HttpNotFound();

                IList<GenereCheckBox> generes = new List<GenereCheckBox>();

                foreach (var item in Context.Generes.Where(g => g.IsActive == true).ToList())
                {
                    var itemIsSelected = Context.AlbumGeneres
                        .Where(x => x.GenereID == item.GenereID && x.AlbumID == album.AlbumID)
                        .FirstOrDefault<AlbumGenere>();

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

                return View("AlbumForm", new CreateAlbumPostForm()
                {
                    IsNew = false,
                    Title = album.AlbumTitle,
                    AlbumID = album.AlbumID,
                    IsActive = album.IsActive,
                    Generes = generes
                });
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AlbumForm(CreateAlbumPostForm model)
        {
            model.IsNew = model.AlbumID == null;

            using (var Context = new ProjectDBEntities())
            {
                if (!ModelState.IsValid)
                    return View(model);

                var selectedGeneres = model.Generes.Where(x => x.IsChecked).ToList();

                var userGuid = (Context.SiteUsers.Where(ax => ax.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

                if (model.IsNew)
                {
                    if (model.AlbumCover == null)
                        ModelState.AddModelError("AlbumCover", "Album Cover required..");

                    if (!ModelState.IsValid)
                        return View(model);

                    if (!VerifyPictureExtension(model.AlbumCover.FileName))
                        ModelState.AddModelError("TrackCover", "Only \".jpg\" & \".png\" files are allowed for cover..");

                    if (!ModelState.IsValid)
                        return View(model);

                    var albumCoverName = Guid.NewGuid() + Path.GetExtension(model.AlbumCover.FileName);
                    var albumCoverUploadPath = Server.MapPath("~/users/albums/images");
                    model.AlbumCover.SaveAs(Path.Combine(albumCoverUploadPath, albumCoverName));
                    var albumCoverPathForDb = "/users/albums/images/" + albumCoverName;

                    Album album = new Album()
                    {
                        AlbumTitle = model.Title,
                        AlbumCoverPath = albumCoverPathForDb,
                        DateAdded = DateTime.UtcNow,
                        IsActive = model.IsActive,
                        UserID = userGuid
                    };

                    Context.Albums.Add(album);

                    Context.SaveChanges();

                    foreach (var item in selectedGeneres)
                    {
                        Context.AlbumGeneres.Add(new AlbumGenere
                        {
                            AlbumID = album.AlbumID,
                            GenereID = (int)item.ID
                        });
                    }

                    Context.SaveChanges();

                }
                else
                {
                    var album = Context.Albums.Where(tx => tx.AlbumID == model.AlbumID).FirstOrDefault<Album>();

                    string albumCoverPathForDb = null;

                    string oldCover = null;

                    if (model.AlbumCover != null)
                    {
                        if (model.AlbumCover == null)
                            ModelState.AddModelError("AlbumCover", "Album Cover required..");

                        if (!ModelState.IsValid)
                            return View(model);

                        if (!VerifyPictureExtension(model.AlbumCover.FileName))
                            ModelState.AddModelError("TrackCover", "Only \".jpg\" & \".png\" files are allowed for cover..");


                        if (!ModelState.IsValid)
                            return View(model);

                        oldCover = album.AlbumCoverPath;
                        var albumCoverName = Guid.NewGuid() + Path.GetExtension(model.AlbumCover.FileName);
                        var albumCoverUploadPath = Server.MapPath("~/users/albums/images");
                        model.AlbumCover.SaveAs(Path.Combine(albumCoverUploadPath, albumCoverName));
                        albumCoverPathForDb = "/users/albums/images/" + albumCoverName;
                    }
                    else
                        albumCoverPathForDb = album.AlbumCoverPath;

                    album.AlbumTitle = model.Title;
                    album.AlbumID = (Guid)model.AlbumID;
                    album.AlbumCoverPath = albumCoverPathForDb;
                    album.DateAdded = DateTime.UtcNow;
                    album.IsActive = model.IsActive;
                    album.UserID = userGuid;

                    foreach (var item in Context.AlbumGeneres.Where(x => x.AlbumID == album.AlbumID).ToList())
                    {
                        Context.AlbumGeneres.Remove(item);
                    }

                    foreach (var item in selectedGeneres)
                    {
                        Context.AlbumGeneres.Add(new AlbumGenere
                        {
                            AlbumID = album.AlbumID,
                            GenereID = (int)item.ID
                        });
                    }
                    
                    Context.SaveChanges();

                    if (oldCover != null)
                        RemoveOld(oldCover);
                }
            }
            return RedirectToAction("ListAlbums");
        }

        [Authorize(Roles = "user")]
        public ActionResult AddSongsInAlbum()
        {
            using (var Context = new ProjectDBEntities())
            {
                var userID = (Context.SiteUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

                ViewBag.Albums = Context.Albums
                    .Where(x => x.UserID == userID && x.IsActive == true && x.UserID == userID)
                    .ToList();

                ViewBag.SharingTypes = Context.SharingTypes.ToList();

                return View(new AddSongsInAlbumViewModel
                {
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
        }

        [Authorize(Roles = "user")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddSongsInAlbum(AddSongsInAlbumViewModel model)
        {
            var Context = new ProjectDBEntities();

            var userID = (Context.SiteUsers.Where(ax => ax.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

            ViewBag.Albums = Context.Albums
                .Where(x => x.UserID == userID && x.IsActive == true && x.UserID == userID)
                .ToList();

            ViewBag.SharingTypes = Context.SharingTypes.ToList();


            if (!ModelState.IsValid)
                return View(model);

            var selectedGeneres = model.Generes.Where(x => x.IsChecked).ToList();

            var album = Context.Albums
                .Where(x => x.AlbumID == model.AlbumID && x.AlbumTitle != "Default")
                .FirstOrDefault<Album>();

            if (album == null)
                ModelState.AddModelError("Album", "Album ID is not Correct..");

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
                AlbumID = model.AlbumID,
                TrackPath = trackPathForDb,
                TrackCoverPath = trackCoverPathForDb,
                DateAdded = DateTime.UtcNow,
                IsActive = true,
                UserID = userID
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

            ViewBag.Message = "Track added..";

            return View(new AddSongsInAlbumViewModel
            {
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

        private const int AlbumsPerPage = 5;
        [Authorize(Roles = "user")]
        public ActionResult ListAlbums(int page = 1)
        {
            if (page == 0)
                page = 1;

            var Context = new ProjectDBEntities();

            var userID = (Context.SiteUsers.Where(sux => sux.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

            var totalAlbumsCount = Context.Albums.Count();

            var currentAlbumPage = Context.Albums
                .Where(x => x.UserID == userID && x.AlbumTitle != "Default")
                .OrderBy(x => x.DateAdded)
                .Skip((page - 1) * AlbumsPerPage)
                .Take(AlbumsPerPage)
                .ToList();

            return View(new AlbumsListViewModel
            {
                Albums = new PageData<Album>(currentAlbumPage, totalAlbumsCount, page, AlbumsPerPage)
            });
        }

        [Authorize(Roles = "user")]
        public ActionResult DeleteAlbum(Guid albumId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers
                    .Where(sxu => sxu.UserName == User.Identity.Name)
                    .FirstOrDefault<SiteUser>()
                    ).UserID;

                var album = Context.Albums
                    .Where(nx => nx.AlbumID == albumId && nx.UserID == guid && nx.AlbumTitle != "Default")
                    .FirstOrDefault<Album>();

                if (album == null)
                    return HttpNotFound();
                else
                {
                    Context.Albums.Remove(album);
                    Context.SaveChanges();
                    RemoveOld(album.AlbumCoverPath);
                }
                return RedirectToAction("ListAlbums");
            }
        }

        [Authorize(Roles = "user")]
        public ActionResult TrashAlbum(Guid albumId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers.Where(sxu => sxu.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;

                var album = Context.Albums.Where(nx => nx.AlbumID == albumId && nx.UserID == guid).FirstOrDefault<Album>();

                if (album == null)
                    return HttpNotFound();

                album.IsActive = false;
                Context.SaveChanges();

                return RedirectToAction("ListAlbums");
            }
        }

        [Authorize(Roles = "user")]
        public ActionResult RestoreAlbum(Guid albumId)
        {
            using (var Context = new ProjectDBEntities())
            {
                var guid = (Context.SiteUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault<SiteUser>()).UserID;
                var album = Context.Albums.Where(x => x.AlbumID == albumId && x.UserID == guid).FirstOrDefault<Album>();

                if (album == null)
                    return HttpNotFound();

                album.IsActive = true;
                Context.SaveChanges();

                return RedirectToAction("ListAlbums");
            }
        }

        //Helper Methods..
        private void RemoveOld(string path)
        {
            string mappedPath = Server.MapPath("~" + path);
            System.IO.File.Delete(mappedPath);
        }

        private bool VerifyMP3Extension(string fileName)
        {
            if (Path.GetExtension(fileName) == ".mp3")
                return true;
            return false;
        }

        private bool VerifyPictureExtension(string fileName)
        {
            if (Path.GetExtension(fileName) == ".jpg" || Path.GetExtension(fileName) == ".png")
                return true;
            return false;
        }

        public ActionResult Play(Guid id)
        {
            //check if track exist..
            var Context = new ProjectDBEntities();

            if (!Context.Albums.Any(x => x.AlbumID == id))
                return HttpNotFound();

            //check if selected album is active..
            if (Context.Albums.Where(x => x.IsActive == true && x.AlbumID == id && x.AlbumTitle != "Default").FirstOrDefault<Album>() == null)
                return HttpNotFound();

            if (User.Identity.IsAuthenticated)
            {
                //get all track of album bt not private and active..
                var allTracks = from a in Context.Albums
                                join t in Context.Tracks on a.AlbumID equals t.AlbumID
                                join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                join st in Context.SharingTypes on uts.SharingTypeID equals st.SharingTypeID
                                where a.AlbumID == id && uts.SharingType.SharingTypeDetail != "Private" && t.IsActive == true
                                select t;

                var tracksList = allTracks.ToList();

                if (tracksList.Count == 0)
                    return HttpNotFound();

                foreach (var item in tracksList)
                {
                    item.LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];
                }


                ViewBag.ActiveTrack = tracksList[0];


                return View(tracksList);
            }
            else
            {
                //get all track of album bt not private and active..
                var allTracks = from a in Context.Albums
                                join t in Context.Tracks on a.AlbumID equals t.AlbumID
                                join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                join st in Context.SharingTypes on uts.SharingTypeID equals st.SharingTypeID
                                where a.AlbumID == id && uts.SharingType.SharingTypeDetail == "Public" && t.IsActive == true
                                select t;

                var tracksList = allTracks.ToList();

                if (tracksList.Count == 0)
                    return HttpNotFound();

                foreach (var item in tracksList)
                {
                    item.LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];
                }


                ViewBag.ActiveTrack = tracksList[0];

                return View(tracksList);
            }
        }

        public ActionResult PlaySongInAlbum(Guid album, Guid track)
        {
            //check if track exist..
            var Context = new ProjectDBEntities();

            if (!Context.Albums.Any(x => x.AlbumID == album))
                return HttpNotFound();

            //check if selected album is active..
            if (Context.Albums.Where(x => x.IsActive == true && x.AlbumID == album && x.AlbumTitle != "Default").FirstOrDefault<Album>() == null)
                return HttpNotFound();

            if (User.Identity.IsAuthenticated)
            {
                //get all track of album bt not private and active..
                var allTracks = from a in Context.Albums
                                join t in Context.Tracks on a.AlbumID equals t.AlbumID
                                join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                join st in Context.SharingTypes on uts.SharingTypeID equals st.SharingTypeID
                                where a.AlbumID == album && uts.SharingType.SharingTypeDetail != "Private" && t.IsActive == true
                                select t;

                var tracksList = allTracks.ToList();

                if (tracksList.Count == 0)
                    return HttpNotFound();

                foreach (var item in tracksList)
                {
                    item.LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];
                }

                if (track != null)
                {
                    var activeTrack = tracksList.Where(x => x.TrackID == track).FirstOrDefault<Track>();
                    ViewBag.ActiveTrack = activeTrack;
                }
                else
                {
                    ViewBag.ActiveTrack = tracksList[0];
                }

                return View("Play",tracksList);
            }
            else
            {
                //get all track of album bt not private and active..
                var allTracks = from a in Context.Albums
                                join t in Context.Tracks on a.AlbumID equals t.AlbumID
                                join uts in Context.UserTrackShares on t.TrackID equals uts.TrackID
                                join st in Context.SharingTypes on uts.SharingTypeID equals st.SharingTypeID
                                where a.AlbumID == album && uts.SharingType.SharingTypeDetail == "Public" && t.IsActive == true
                                select t;

                var tracksList = allTracks.ToList();

                if (tracksList.Count == 0)
                    return HttpNotFound();

                foreach (var item in tracksList)
                {
                    item.LikesCount = item.UserTrackShares.Select(x => x.UserTrackShareLikes.Count()).ToList()[0];
                }

                if (track != null)
                {
                    var activeTrack = tracksList.Where(x => x.TrackID == track).FirstOrDefault<Track>();

                    if (activeTrack == null)
                        return HttpNotFound();
                    else
                        ViewBag.ActiveTrack = activeTrack;
                }
                else
                {
                    ViewBag.ActiveTrack = tracksList[0];
                }

                return View("Play",tracksList);
            }
        }
    }
}
