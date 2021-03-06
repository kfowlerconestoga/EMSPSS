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
    using System.ComponentModel.DataAnnotations;
    
    public partial class Company
    {
        public Company()
        {
            this.EmployeeTypes = new HashSet<EmployeeType>();
            this.PartTimeEmployees = new HashSet<PartTimeEmployee>();
            this.SeasonalEmployees = new HashSet<SeasonalEmployee>();
            this.ContractEmployees = new HashSet<ContractEmployee>();
            this.TimeCards = new HashSet<TimeCard>();
            this.Pieces = new HashSet<Piece>();
            this.FullTimeEmployees = new HashSet<FullTimeEmployee>();
        }
    
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Company Name", Prompt = "Enter Company Name: ", Description = "Company's Name")]
        [RegularExpression(@"^[-a-zA-Z' ]+$", ErrorMessage = "{0} name may only include '-  a-Z A-Z.")]
        public string CompanyName { get; set; }
        public System.DateTime EnrolledSince { get; set; }
    
        public virtual ICollection<EmployeeType> EmployeeTypes { get; set; }
        public virtual ICollection<PartTimeEmployee> PartTimeEmployees { get; set; }
        public virtual ICollection<SeasonalEmployee> SeasonalEmployees { get; set; }
        public virtual ICollection<ContractEmployee> ContractEmployees { get; set; }
        public virtual ICollection<TimeCard> TimeCards { get; set; }
        public virtual ICollection<Piece> Pieces { get; set; }
        public virtual ICollection<FullTimeEmployee> FullTimeEmployees { get; set; }
    }
}
