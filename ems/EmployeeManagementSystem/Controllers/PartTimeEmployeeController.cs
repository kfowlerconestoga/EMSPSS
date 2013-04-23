using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utilities;

namespace EmployeeManagementSystem.Controllers
{
    public class PartTimeEmployeeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12(); 

        public ViewResult FindDetails(int id)
        {
            db = new EMSEntities12();
            PartTimeEmployee pte = EMSPSSUtilities.GetPartTimer(id);
            ReasonForLeaving rfl = db.ReasonForLeavings.Find(pte.ReasonForLeaving2Id);
            if (rfl != null)
            {
                ViewBag.Reason = rfl.ReasonForLeaving1;
            }

            ViewBag.SIN = EMSPSSUtilities.FormatSIN_BN(pte.Employee.SIN_BN, true);
            return View(pte);
        }

        public ViewResult FindEdit(int id)
        {
            PartTimeEmployee pt = EMSPSSUtilities.GetPartTimer(id);
            return View(pt);
        }

        [HttpPost]
        public ActionResult FindEdit(PartTimeEmployee parttimeemployee)
        {
            db = new EMSEntities12();
            String sin = parttimeemployee.Employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            parttimeemployee.Employee.SIN_BN = sin;
            if (parttimeemployee.DateOfTermination == null)
            {
                parttimeemployee.ReasonForLeaving2Id = null;
            }
            else if (parttimeemployee.DateOfTermination != null && parttimeemployee.ReasonForLeaving2Id == 0)
            {
                ModelState.AddModelError("ReasonForLeaving", "You must enter a reason for leaving.");
            }
            if (!EMSPSSUtilities.VerifySIN(parttimeemployee.Employee.SIN_BN))
            {
                ModelState.AddModelError("SIN_BN", parttimeemployee.Employee.SIN_BN + " is not a valid SIN.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(Convert.ToDateTime("1900-01-01"), parttimeemployee.Employee.DateOfBirth))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be past the year 1900.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(parttimeemployee.Employee.DateOfBirth, DateTime.Now))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be in the past.");
            }
            if (parttimeemployee.DateOfTermination != null)
            {
                DateTime dot = (DateTime)parttimeemployee.DateOfTermination;
                if (!EMSPSSUtilities.DateIsElapsed(parttimeemployee.DateOfHire, dot))
                {
                    ModelState.AddModelError("DOT", "Date of Termination must be in the future from Date of Hire.");
                }
            }
            if (ModelState.IsValid)
            {
                EMSPSSUtilities.AuditExistingEmployee(parttimeemployee.EmployeeRef2Id, parttimeemployee.Employee, User.Identity.Name);
                EMSPSSUtilities.AuditExistingPartTimeEmployee(parttimeemployee.EmployeeRef2Id, parttimeemployee, User.Identity.Name);
                parttimeemployee.Company = db.Companies.Find(parttimeemployee.EmployedWith2Id);

                parttimeemployee.Employee.Completed = EMSPSSUtilities.ValidatePartTimeEmployeeComplete(parttimeemployee);

                db.Entry(parttimeemployee.Employee).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(parttimeemployee).State = EntityState.Modified;
                db.SaveChanges();

                //ModelState.AddModelError( validationError.PropertyName, validationError.ErrorMessage);

                return RedirectToAction("SearchIndex", "Home", new { FirstName = parttimeemployee.Employee.FirstName, LastName = parttimeemployee.Employee.LastName, SINBN = parttimeemployee.Employee.SIN_BN });
            }

            return View(parttimeemployee);
        }
    }
}
