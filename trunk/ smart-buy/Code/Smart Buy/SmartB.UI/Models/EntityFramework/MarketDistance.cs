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
    
    public partial class MarketDistance
    {
        public int FromMarket { get; set; }
        public int ToMarket { get; set; }
        public Nullable<double> Distance { get; set; }
    
        public virtual Market Market { get; set; }
    }
}
