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
    
    public partial class Fieldname
    {
        public Fieldname()
        {
            this.Audits = new HashSet<Audit>();
        }
    
        public int FieldId { get; set; }
        public string Fieldname1 { get; set; }
    
        public virtual ICollection<Audit> Audits { get; set; }
    }
}