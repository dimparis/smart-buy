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
    
    public partial class Profile
    {
        public string Username { get; set; }
        public string FirstRoute { get; set; }
        public string FirstMarkets { get; set; }
        public string FirstRouteName { get; set; }
        public string SecondRoute { get; set; }
        public string SecondMarkets { get; set; }
        public string SecondRouteName { get; set; }
        public string ThirdRoute { get; set; }
        public string ThirdMarkets { get; set; }
        public string ThirdRouteName { get; set; }
    
        public virtual User User { get; set; }
    }
}
