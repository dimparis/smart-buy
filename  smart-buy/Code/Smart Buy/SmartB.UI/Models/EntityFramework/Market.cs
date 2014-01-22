//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SmartB.UI.Models.EntityFramework
{
    using System;
    using System.Collections.Generic;
    
    public partial class Market
    {
        public Market()
        {
            this.ParseInfoes = new HashSet<ParseInfo>();
            this.SellProducts = new HashSet<SellProduct>();
            this.UserPrices = new HashSet<UserPrice>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsActive { get; set; }
    
        public virtual ICollection<ParseInfo> ParseInfoes { get; set; }
        public virtual ICollection<SellProduct> SellProducts { get; set; }
        public virtual ICollection<UserPrice> UserPrices { get; set; }
    }
}
