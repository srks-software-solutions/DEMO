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
    
    public partial class tblpart
    {
        public int PartID { get; set; }
        public int PartNo { get; set; }
        public string PartDesc { get; set; }
        public string PartName { get; set; }
        public int IdleCycleTime { get; set; }
        public int UnitDesc { get; set; }
        public int IsDeleted { get; set; }
        public string CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    
        public virtual tblunit tblunit { get; set; }
    }
}
