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
    
    public partial class UserAddress
    {
        public int UserAddressID { get; set; }
        public string UserAddressDetail { get; set; }
        public int UserAddressTypeID { get; set; }
        public System.Guid UserID { get; set; }
    
        public virtual UserAddressType UserAddressType { get; set; }
    }
}