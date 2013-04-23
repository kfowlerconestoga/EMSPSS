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
    public class ContractEmployeeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        public ViewResult FindDetails(int id)
        {
            db = new EMSEntities12();
            ContractEmployee ce = EMSPSSUtilities.GetContract(id);
            ReasonForLeaving rfl = db.ReasonForLeavings.Find(ce.ReasonForLeaving4Id);
            if (rfl != null)
            {
                ViewBag.Reason = rfl.ReasonForLeaving1;
            }

            ViewBag.BN = EMSPSSUtilities.FormatSIN_BN(ce.Employee.SIN_BN, false);
            return View(ce);
        }

        [Authorize(Roles="Admin")]
        public ViewResult FindEdit(int id)
        {

            ContractEmployee ce = EMSPSSUtilities.GetContract(id);
            return View(ce);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult FindEdit(ContractEmployee contractemployee)
        {
            db = new EMSEntities12();
            String sin = contractemployee.Employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            contractemployee.Employee.Completed = true;
            contractemployee.Employee.SIN_BN = sin;
            if (contractemployee.ReasonForLeaving4Id == 0)
            {
                contractemployee.ReasonForLeaving4Id = null;
            }
            if (!EMSPSSUtilities.VerifySIN(contractemployee.Employee.SIN_BN) || !EMSPSSUtilities.VerifyBusinessNum(contractemployee.Employee.SIN_BN, contractemployee.Employee.DateOfBirth.ToString("yyyy-MM-dd")))
            {
                ModelState.AddModelError("SIN_BN", contractemployee.Employee.SIN_BN + " is not a valid BN.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(Convert.ToDateTime("1600-01-01"), contractemployee.Employee.DateOfBirth))
            {
                ModelState.AddModelError("DOB", "Date of Incorporation must be past the year 1600.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(contractemployee.Employee.DateOfBirth, DateTime.Now))
            {
                ModelState.AddModelError("DOB", "Date of Incorporation must be in the past.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(contractemployee.Employee.DateOfBirth, contractemployee.ContractStartDate))
            {
                ModelState.AddModelError("CSD", "Contract Start date must be in the future from date of incorporation.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(contractemployee.ContractStartDate, contractemployee.ContractStopDate))
            {
                ModelState.AddModelError("CSD2", "Contract Stop date must be in the future from Contract Start date.");
            }
            if (ModelState.IsValid)
            {
                EMSPSSUtilities.AuditExistingEmployee(contractemployee.EmployeeRef4Id, contractemployee.Employee, User.Identity.Name);
                EMSPSSUtilities.AuditExistingContractEmployee(contractemployee.EmployeeRef4Id, contractemployee, User.Identity.Name);

                

                contractemployee.Company = db.Companies.Find(contractemployee.EmployedWith4Id);

                db.Entry(contractemployee.Employee).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(contractemployee).State = EntityState.Modified;
                db.SaveChanges();

                //ModelState.AddModelError( validationError.PropertyName, validationError.ErrorMessage);

                return RedirectToAction("SearchIndex", "Home", new { FirstName = contractemployee.Employee.FirstName, LastName = contractemployee.Employee.LastName, SINBN = contractemployee.Employee.SIN_BN });
            }
            return View(contractemployee);
        }
    }
}
