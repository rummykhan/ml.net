using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.ViewModels
{
    public class ShareSongPostForm
    {
        public bool IsNew { get; set; }
        public Guid? ID { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        public Guid? AlbumID { get; set; }

        [Display(Name = "Track (a .mp3 file..)")]
        public HttpPostedFileBase Track { get; set; }

        public string TrackPath { get; set; }

        [Display(Name = "Track Cover (a .jpg or .png file)")]
        public HttpPostedFileBase TrackCover { get; set; }

        public string TrackCoverPath { get; set; }

        public DateTime DateAdded { get; set; }

        [Display(Name="Sharing Type")]
        public int SharingTypeID { get; set; }

        public int TrackShareID { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; }

        public Guid UserID { get; set; }

        public IList<GenereCheckBox> Generes { get; set; }

    }

    public class SongsListViewModel
    {
        public PageData<Track> Tracks { get; set; }
    }

    public class GenereCheckBox
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SongPlay
    {
        public Guid ID { get; set; }

        public string Title { get; set; }

        public string AlbumTitle { get; set; }

        public string TrackPath { get; set; }

        public string TrackCoverPath { get; set; }

        public DateTime DateAdded { get; set; }

        public string SharingType { get; set; }

        public SiteUser Owner { get; set; }

        public List<string> Generes { get; set; }
    }


}