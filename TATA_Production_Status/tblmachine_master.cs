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
    
    public partial class tblmachine_master
    {
        public int MachineID { get; set; }
        public string MachineInvNo { get; set; }
        public string IPAddress { get; set; }
        public Nullable<int> MachineType { get; set; }
        public string ControllerType { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
