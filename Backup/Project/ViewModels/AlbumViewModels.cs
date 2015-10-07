using Project.Infrastructure;
using Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.ViewModels
{
    public class CreateAlbumPostForm
    {
        public bool IsNew { get; set; }
        public Guid? AlbumID { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Display(Name = "Album Cover (a .jpg or .png file)")]
        public HttpPostedFileBase AlbumCover { get; set; }

        public string AlbumCoverPath { get; set; }

        public DateTime DateAdded { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; }

        public Guid UserID { get; set; }

        public IList<GenereCheckBox> Generes { get; set; }

    }

    public class AlbumsListViewModel
    {
        public PageData<Album> Albums { get; set; }
    }

    public class AddSongsInAlbumViewModel
    {
        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, Display(Name = "Select Album")]
        public Guid AlbumID { get; set; }

        [Required, Display(Name = "Track (a .mp3 file..)")]
        public HttpPostedFileBase Track { get; set; }

        public string TrackPath { get; set; }

        [Required, Display(Name = "Track Cover (a .jpg or .png file)")]
        public HttpPostedFileBase TrackCover { get; set; }

        public string TrackCoverPath { get; set; }

        public DateTime DateAdded { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Sharing Type")]
        public int SharingTypeID { get; set; }

        public Guid UserID { get; set; }

        public IList<GenereCheckBox> Generes { get; set; }
    }

    public class AlbumIndexViewModel
    {
        public IList<Album> Albums { get; set; }
    }
}