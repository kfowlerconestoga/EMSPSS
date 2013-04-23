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
    public class SeasonalEmployeeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        public ViewResult FindDetails(int id)
        {
            db = new EMSEntities12();
            SeasonalEmployee se = EMSPSSUtilities.GetSeasonal(id);
            ReasonForLeaving rfl = db.ReasonForLeavings.Find(se.ReasonForLeaving3Id);
            if (rfl != null)
            {
                ViewBag.Reason = rfl.ReasonForLeaving1;
            }
            ViewBag.SIN = EMSPSSUtilities.FormatSIN_BN(se.Employee.SIN_BN, true);
            return View(se);
        }

        public ViewResult FindEdit(int id)
        {
            SeasonalEmployee se = EMSPSSUtilities.GetSeasonal(id);
            return View(se);
        }

        [HttpPost]
        public ActionResult FindEdit(SeasonalEmployee seasonalemployee)
        {
            db = new EMSEntities12();
            if (seasonalemployee.ReasonForLeaving3Id == 0)
            {
                seasonalemployee.ReasonForLeaving3Id = null;
            }
            String sin = seasonalemployee.Employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            seasonalemployee.Employee.SIN_BN = sin;
            if (!EMSPSSUtilities.VerifySIN(seasonalemployee.Employee.SIN_BN))
            {
                ModelState.AddModelError("SIN_BN", seasonalemployee.Employee.SIN_BN + " is not a valid SIN.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(Convert.ToDateTime("1900-01-01"), seasonalemployee.Employee.DateOfBirth))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be past the year 1900.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(seasonalemployee.Employee.DateOfBirth, DateTime.Now))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be in the past.");
            }
            
            if (ModelState.IsValid)
            {
                EMSPSSUtilities.AuditExistingEmployee(seasonalemployee.EmployeeRef3Id, seasonalemployee.Employee, User.Identity.Name);
                EMSPSSUtilities.AuditExistingSeasonalEmployee(seasonalemployee.EmployeeRef3Id, seasonalemployee, User.Identity.Name);
                


                seasonalemployee.Company = db.Companies.Find(seasonalemployee.EmployedWith3Id);

                seasonalemployee.Employee.Completed = EMSPSSUtilities.ValidateSeasonalEmployeeComplete(seasonalemployee);

                db.Entry(seasonalemployee.Employee).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(seasonalemployee).State = EntityState.Modified;
                db.SaveChanges();

                //ModelState.AddModelError( validationError.PropertyName, validationError.ErrorMessage);

                return RedirectToAction("SearchIndex", "Home", new { FirstName = seasonalemployee.Employee.FirstName, LastName = seasonalemployee.Employee.LastName, SINBN = seasonalemployee.Employee.SIN_BN });
            }

            return View(seasonalemployee);
        }
    }
}
