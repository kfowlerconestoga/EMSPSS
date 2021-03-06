//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EmployeeManagementSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Audit
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public System.DateTime DateTime { get; set; }
        public Nullable<int> AuditTypeId { get; set; }
        public Nullable<int> FieldId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int EmployeeId { get; set; }
    
        public virtual AuditAction AuditAction { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Fieldname Fieldname { get; set; }
        public virtual User User { get; set; }
    }
}
