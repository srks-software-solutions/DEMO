//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TATA_Production_Status
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbldowntimedetail
    {
        public int DT_ID { get; set; }
        public int DTCategoryID { get; set; }
        public string DTReason { get; set; }
        public string DTDesc { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    
        public virtual tbldowntimecategory tbldowntimecategory { get; set; }
    }
}
