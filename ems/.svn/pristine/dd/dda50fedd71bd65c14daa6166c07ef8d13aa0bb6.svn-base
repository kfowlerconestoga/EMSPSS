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
    public class FullTimeEmployeeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();       

        public ViewResult FindDetails(int id)
        {
            db = new EMSEntities12();
            FullTimeEmployee fulltimeemployee = EMSPSSUtilities.GetFullTimer(id);            
            ReasonForLeaving rfl = db.ReasonForLeavings.Find(fulltimeemployee.ReasonForLeavingId);
            if (rfl != null)
            {
                ViewBag.Reason = rfl.ReasonForLeaving1;
            }

            ViewBag.SIN = EMSPSSUtilities.FormatSIN_BN(fulltimeemployee.Employee.SIN_BN, true);
            return View(fulltimeemployee);
        }

        public ViewResult FindEdit(int id)
        {
            FullTimeEmployee ft = EMSPSSUtilities.GetFullTimer(id);
            return View(ft);
        }

        [HttpPost]
        public ActionResult FindEdit(FullTimeEmployee fulltimeemployee)
        {
            db = new EMSEntities12();
            String sin = fulltimeemployee.Employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            fulltimeemployee.Employee.SIN_BN = sin;
            if (fulltimeemployee.DateOfTermination == null)
            {
                fulltimeemployee.ReasonForLeavingId = null;
            }
            else if (fulltimeemployee.DateOfTermination != null && fulltimeemployee.ReasonForLeavingId == 0)
            {
                ModelState.AddModelError("ReasonForLeaving", "You must enter a reason for leaving.");
            }
            if (!EMSPSSUtilities.VerifySIN(fulltimeemployee.Employee.SIN_BN))
            {
                ModelState.AddModelError("SIN_BN", fulltimeemployee.Employee.SIN_BN + " is not a valid SIN.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(Convert.ToDateTime("1900-01-01"), fulltimeemployee.Employee.DateOfBirth))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be past the year 1900.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(fulltimeemployee.Employee.DateOfBirth, DateTime.Now))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be in the past.");
            }
            if (fulltimeemployee.DateOfTermination != null)
            {
                DateTime dot = (DateTime)fulltimeemployee.DateOfTermination;
                if (!EMSPSSUtilities.DateIsElapsed(fulltimeemployee.DateOfHire, dot))
                {
                    ModelState.AddModelError("DOT", "Date of Termination must be in the future from Date of Hire.");
                }
            }
            if (ModelState.IsValid)
            {
                EMSPSSUtilities.AuditExistingEmployee(fulltimeemployee.EmployeeRefId, fulltimeemployee.Employee, User.Identity.Name);
                EMSPSSUtilities.AuditExistingFullTimeEmployee(fulltimeemployee.EmployeeRefId, fulltimeemployee, User.Identity.Name);

                fulltimeemployee.Employee.Completed = EMSPSSUtilities.ValidateFullTimeEmployeeComplete(fulltimeemployee);

                fulltimeemployee.Company = db.Companies.Find(fulltimeemployee.EmployedWithId);

                db.Entry(fulltimeemployee.Employee).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(fulltimeemployee).State = EntityState.Modified;
                db.SaveChanges();
                
                //ModelState.AddModelError( validationError.PropertyName, validationError.ErrorMessage);
                
                return RedirectToAction("SearchIndex", "Home", new { FirstName = fulltimeemployee.Employee.FirstName, LastName = fulltimeemployee.Employee.LastName, SINBN = fulltimeemployee.Employee.SIN_BN });
            }
            
            return View(fulltimeemployee);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}