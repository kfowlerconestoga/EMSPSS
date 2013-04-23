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
    public class TimeCardController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        //
        // GET: /TimeCard/


        public ActionResult Home()
        {
            ViewBag.compList = EMSPSSUtilities.GetCompList();
            ViewBag.empList = new List<Employee>();
            return View();
        }


        [HttpPost]
        public ActionResult Home(string compList)
        {
            db = new EMSEntities12();
            if (compList == "")
            {
                List<Employee> le = db.Employees.ToList();
                for (int c = 0; c < le.Count; c++)
                {
                    if (le[c].ContractEmployees.Count > 0 || !le[c].Active || !le[c].Completed || le[c].ContractEmployees.Count > 1)
                    {
                        le.Remove(le[c]);
                        c--;
                    }
                }
                ViewBag.empList = le;
            }
            else
            {
                List<Employee> le = new List<Employee>();
                List<EmployeeType> let = db.EmployeeTypes.ToList();
                for (int c = 0; c < let.Count; c++)
                {
                    if (let[c].Company.CompanyName == compList && let[c].Employee.Active && let[c].Employee.Completed && let[c].Employee.ContractEmployees.Count < 1)
                    {
                        le.Add(let[c].Employee);
                    }
                }
                ViewBag.empList = le;
            }
            ViewBag.compList = EMSPSSUtilities.GetCompList();
            return View();
        }

        public ActionResult Index(int EmployeeId)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            List<TimeCard> tcList = new List<TimeCard>();
            foreach (Employee emp in db.Employees)
            {
                if (emp.EmployeeId == EmployeeId)
                {
                    ViewBag.Name = emp.FirstName + " " + emp.LastName;
                    break;
                }
            }
            foreach (Company cmp in db.Companies)
            {
                if (cmp.CompanyId == CompanyId)
                {
                    ViewBag.Company = cmp.CompanyName;
                    break;
                }
            }
            ViewBag.EmployeeId = EmployeeId;
            ViewBag.CompanyId = CompanyId;

            foreach (TimeCard tc in db.TimeCards)
            {
                if ((tc.EmployeeRef5Id == EmployeeId) && (tc.EmployedWith5Id == CompanyId))
                {
                    tcList.Add(tc);
                }
            }

            return View(tcList);
        }

        public ActionResult Details(int EmployeeId, DateTime weekOf)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            // test if weekOf is a monday, if not, go to employee's timecard index
            if (weekOf.Date.DayOfWeek != DayOfWeek.Monday)
            {
                return RedirectToAction("Index", "TimeCard", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
            }

            foreach (TimeCard tmp in db.TimeCards)
            {
                if ((tmp.EmployeeRef5Id == EmployeeId) && (tmp.EmployedWith5Id == CompanyId) && (tmp.WeekOf == weekOf))
                {
                    return View(tmp);
                }
            }

            // if it reaches here, the timecard was not found return to employee's timecard index
            foreach (Employee emp in db.Employees)
            {
                if (emp.EmployeeId == EmployeeId)
                {
                    ViewBag.Name = emp.FirstName + " " + emp.LastName;
                    break;
                }
            }
            foreach (Company cmp in db.Companies)
            {
                if (cmp.CompanyId == CompanyId)
                {
                    ViewBag.Company = cmp.CompanyName;
                    break;
                }
            }
            ViewBag.EmployeeId = EmployeeId;
            ViewBag.CompanyId = CompanyId;

            return RedirectToAction("Index", "TimeCard", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
        }

        public ActionResult Edit(int EmployeeId, DateTime? timeCardDate)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            if (timeCardDate != null)
            {


                DateTime weekOf = (DateTime)timeCardDate.Value;
                while (weekOf.DayOfWeek != DayOfWeek.Monday)
                {
                    weekOf = weekOf.AddDays(-1);
                }

                TimeCard tc = new TimeCard();
                List<TimeCard> allTCs = db.TimeCards.ToList();

                foreach (TimeCard tmp in allTCs)
                {
                    if ((tmp.EmployeeRef5Id == EmployeeId) && (tmp.EmployedWith5Id == CompanyId) && (tmp.WeekOf == weekOf))
                    {
                        return View(tmp);
                    }
                }
                // if it reaches here, the timecard was not found
                // create and add it before proceeding

                tc.EmployedWith5Id = CompanyId;
                tc.EmployeeRef5Id = EmployeeId;
                tc.Employee = db.Employees.Find(EmployeeId);
                tc.Company = db.Companies.Find(CompanyId);
                tc.WeekOf = weekOf;
                tc.Mon = null;
                tc.Tue = null;
                tc.Wed = null;
                tc.Thu = null;
                tc.Fri = null;
                tc.Sat = null;
                tc.Sun = null;

                db.TimeCards.Add(tc);
                db.SaveChanges();

                return View(tc);
            }
            return RedirectToAction("Index", "TimeCard", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
        }

        [HttpPost]
        public ActionResult Edit(TimeCard timecard)
        {
            db = new EMSEntities12();
            if (ModelState.IsValid)
            {
                db.Entry(timecard).State = EntityState.Modified;
                db.SaveChanges(); 
                foreach (Employee emp in db.Employees)
                {
                    if (emp.EmployeeId == timecard.EmployeeRef5Id)
                    {
                        ViewBag.Name = emp.FirstName + " " + emp.LastName;
                        break;
                    }
                }
                foreach (Company cmp in db.Companies)
                {
                    if (cmp.CompanyId == timecard.EmployedWith5Id)
                    {
                        ViewBag.Company = cmp.CompanyName;
                        break;
                    }
                }
                ViewBag.EmployeeId = timecard.EmployeeRef5Id;
                ViewBag.CompanyId = timecard.EmployedWith5Id;
                return RedirectToAction("Index", "TimeCard", new { EmployeeId = timecard.EmployeeRef5Id, CompanyId = timecard.EmployedWith5Id });
            }
            return View(timecard);
        }

    }
}
