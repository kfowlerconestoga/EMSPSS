using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace EmployeeManagementSystem.Models
{
    public class Profile : ProfileBase
    {
        [Display(Name = "First Name")]
        public virtual string FirstName
        {
            get
            {
                return (this.GetPropertyValue("FirstName").ToString());
            }
            set
            {
                this.SetPropertyValue("FirstName", value);
            }
        }

        [Display(Name = "Last Name")]
        public virtual string LastName
        {
            get
            {
                return (this.GetPropertyValue("LastName").ToString());
            }
            set
            {
                this.SetPropertyValue("LastName", value);
            }
        }

        [Display(Name = "IsAdmin")]
        public virtual Boolean IsAdmin
        {
            get
            {
                return ((Boolean)this.GetPropertyValue("IsAdmin"));
            }
            set
            {
                this.SetPropertyValue("Admin", value);
            }
        }

        public static Profile GetProfile(string username)
        {
            return Create(username) as Profile;
        }
    }
}