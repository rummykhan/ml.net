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
    
    public partial class TrackGenere
    {
        public int Id { get; set; }
        public System.Guid TrackId { get; set; }
        public int GenereID { get; set; }
    
        public virtual Genere Genere { get; set; }
        public virtual Track Track { get; set; }
    }
}
