//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Project.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserTrackShare
    {
        public UserTrackShare()
        {
            this.UserTrackShareLikes = new HashSet<UserTrackShareLike>();
        }
    
        public int UserTrackSharedID { get; set; }
        public System.Guid TrackID { get; set; }
        public int SharingTypeID { get; set; }
        public System.DateTime SharingDate { get; set; }
    
        public virtual SharingType SharingType { get; set; }
        public virtual Track Track { get; set; }
        public virtual ICollection<UserTrackShareLike> UserTrackShareLikes { get; set; }
    }
}