using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utilities;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        private Dictionary<String, String> GetOptions()
        {
            Dictionary<String, String> employees = new Dictionary<String, String>();
            employees.Add("Seasonal", "Seasonal Employee");
            employees.Add("FullTime", "Full-Time Employee");
            employees.Add("PartTime", "Part-Time Employee");

            return employees;
        }

        public ActionResult EditUnknownType(int id)
        {
            String controller = EMSPSSUtilities.GetEmployeeType(id);
            return RedirectToAction("FindEdit", controller, new { id = id });           
        }

        public ActionResult DetailsOfUnknownType(int id)
        {
            String controller = EMSPSSUtilities.GetEmployeeType(id);
            return RedirectToAction("FindDetails", controller, new { id = id }); 
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminHome()
        {
            db = new EMSEntities12();
            Employee employee = new Employee();
            List<Employee> emplist = new List<Employee>();
          
          foreach(FullTimeEmployee FtCompleted in db.FullTimeEmployees )
          {
              if (!FtCompleted.Employee.Completed)
              {
                  emplist.Add(FtCompleted.Employee);

              }
          }
          foreach (PartTimeEmployee PtCompleted in db.PartTimeEmployees)
          {
              if (!PtCompleted.Employee.Completed)
              {
                  emplist.Add(PtCompleted.Employee);

              }
          }
          foreach (SeasonalEmployee SesonCompleted in db.SeasonalEmployees)
          {
              if (!SesonCompleted.Employee.Completed)
              {
                  emplist.Add(SesonCompleted.Employee);

              }
          }

          return View(emplist);
        }



        public ActionResult EmployeeCreation()
        {
            Employee employee = new Employee();
            ViewBag.Employees = GetOptions();
            return View(employee);
        }

        //
        // POST: 

        [HttpPost]
        public ActionResult EmployeeCreation(Employee employee, string DDlist )
        {
            db = new EMSEntities12();
            ViewBag.Employees = GetOptions();
            String sin = employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            employee.SIN_BN = sin;
            employee.Completed = false;
            if (!EMSPSSUtilities.VerifySIN(employee.SIN_BN))
            {
                ModelState.AddModelError("SIN_BN", employee.SIN_BN + " is not a valid BN.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(Convert.ToDateTime("1900-01-01"), employee.DateOfBirth))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be past the year 1900.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(employee.DateOfBirth, DateTime.Now))
            {
                ModelState.AddModelError("DOB", "Date of Birth must be in the past.");
            }
            if (ModelState.IsValid)
            {
                if ("Seasonal" == DDlist)
                {
                    return RedirectToAction("Seasonal", new { name = employee.FirstName, last = employee.LastName, day = employee.DateOfBirth.Day.ToString(), month = employee.DateOfBirth.Month.ToString(), year = employee.DateOfBirth.Year.ToString(), sin = employee.SIN_BN });
                }
                else if ("PartTime" == DDlist)
                {
                    return RedirectToAction("PartTime", new { name = employee.FirstName, last = employee.LastName, day = employee.DateOfBirth.Day.ToString(), month = employee.DateOfBirth.Month.ToString(), year = employee.DateOfBirth.Year.ToString(), sin = employee.SIN_BN });
                }
                else if ("FullTime" == DDlist)
                {
                    return RedirectToAction("Fulltime", new { name = employee.FirstName, last = employee.LastName, day = employee.DateOfBirth.Day.ToString(), month = employee.DateOfBirth.Month.ToString(), year = employee.DateOfBirth.Year.ToString(), sin = employee.SIN_BN });
                }
            }
                      
        return View(employee);
        }

        public ActionResult Fulltime(Employee employee, string name, string last, string day, string month, string year, string sin)
        {
            FullTimeEmployee FTemployee = new FullTimeEmployee();
            ViewBag.Employees = GetOptions();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            return View(FTemployee);
        }

        //
        // POST: 

        [HttpPost]
        public ActionResult Fulltime(FullTimeEmployee FTemployee, string CompList, string name, string last, string day, string month, string year, string sin)
        {
            db = new EMSEntities12();
            EmployeeType ETemployee = new EmployeeType();
            Employee employee = new Employee();
            ViewBag.Employees = GetOptions();

            employee.FirstName = name;
            employee.LastName = last;
            int Year = 0, Month = 0, Day = 0;

            Int32.TryParse(year,out Year);
            Int32.TryParse(day,out Day);
            Int32.TryParse(month,out Month);

            employee.DateOfBirth = new DateTime(Year, Month, Day);
            employee.SIN_BN = sin;
            employee.Completed = false;

            FTemployee.EmployeeRefId = employee.EmployeeId;
            
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    FTemployee.EmployedWithId = mycmp.CompanyId;
                }
            }
            ETemployee.CompanyId = FTemployee.EmployedWithId;
            
            ETemployee.EmployeeType1 = "Full Time";
            ETemployee.DateofHire = FTemployee.DateOfHire;


            if (CompList == "")
            {
                ModelState.AddModelError("comp", "This field is Required.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(employee.DateOfBirth, FTemployee.DateOfHire, 18))
            {
                ModelState.AddModelError("DOH", "A full time employee must be at least 18 years old.");
            }
            if (ModelState.IsValid)
            {

                db.Employees.Add(employee);
                db.SaveChanges();
                EMSPSSUtilities.AuditNewBaseEmployee(employee, User.Identity.Name);
                FTemployee.EmployeeRefId = employee.EmployeeId;
                ETemployee.EmployeeId = FTemployee.EmployeeRefId;
                EMSPSSUtilities.CompareFields("", FTemployee.DateOfHire.ToString(), 11, 1, FTemployee.EmployeeRefId, User.Identity.Name);

                db.FullTimeEmployees.Add(FTemployee);
                db.EmployeeTypes.Add(ETemployee);
                db.SaveChanges();
                return RedirectToAction("SearchIndex", "Home", new { FirstName = FTemployee.Employee.FirstName, LastName = FTemployee.Employee.LastName, SINBN = FTemployee.Employee.SIN_BN });
            }

            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            return View(FTemployee);
        }


        public ActionResult Seasonal(string name, string last, string day, string month, string year, string sin)
        {
            SeasonalEmployee employee = new SeasonalEmployee();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            ViewBag.Employees = GetOptions();
            
            ViewBag.SeasonName = EMSPSSUtilities.GetSeasonList();
            ViewBag.Employees = GetOptions();
            return View(employee);
        }

        //
        // POST: 
        
        [HttpPost]
        public ActionResult Seasonal(SeasonalEmployee Semployee, string CompList, string SeasonLst, string name, string last, string day, string month, string year, string sin)
        {
            db = new EMSEntities12();
            ViewBag.SeasonName = EMSPSSUtilities.GetSeasonList();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            EmployeeType ETemployee = new EmployeeType();
            ViewBag.Employees = GetOptions();

            Employee employee = new Employee();
            employee.FirstName = name;
            employee.LastName = last;
            int Year = 0, Month = 0, Day = 0;

            Int32.TryParse(year, out Year);
            Int32.TryParse(day, out Day);
            Int32.TryParse(month, out Month);

            employee.DateOfBirth = new DateTime(Year, Month, Day);
            employee.SIN_BN = sin;
            employee.Completed = false;

            
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    Semployee.EmployedWith3Id = mycmp.CompanyId;
                }
            }

            foreach (Season myseas in db.Seasons)
            {
                if (myseas.Season1 == SeasonLst)
                {
                    Semployee.SeasonId = myseas.SeasonId;
                }
            }

            ETemployee.CompanyId = Semployee.EmployedWith3Id;
            ETemployee.EmployeeType1 = "Seasonal";
            ETemployee.DateofHire = Semployee.SeasonYear;
            if (CompList == "")
            {
                ModelState.AddModelError("comp", "This field is Required.");
            }
            if (SeasonLst == "")
            {
                ModelState.AddModelError("season", "This field is Required.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(employee.DateOfBirth, Semployee.SeasonYear, 18))
            {
                ModelState.AddModelError("DOH", "A seasonal employee must be at least 18 years old.");
            }
            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                EMSPSSUtilities.AuditNewBaseEmployee(employee, User.Identity.Name);
                Semployee.EmployeeRef3Id = employee.EmployeeId;
                ETemployee.EmployeeId = Semployee.EmployeeRef3Id;
                EMSPSSUtilities.CompareFields("", Semployee.SeasonId.ToString(), 17, 1, Semployee.EmployeeRef3Id, User.Identity.Name);

                db.SeasonalEmployees.Add(Semployee);
                db.EmployeeTypes.Add(ETemployee);
                db.SaveChanges();
                return RedirectToAction("SearchIndex", "Home", new { FirstName = Semployee.Employee.FirstName, LastName = Semployee.Employee.LastName, SINBN = Semployee.Employee.SIN_BN });
            }

            return View(Semployee);
        }


        public ActionResult PartTime(string name, string last, string day, string month, string year, string sin)
        {
            PartTimeEmployee employee = new PartTimeEmployee();
            ViewBag.Employees = GetOptions();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            return View(employee);
        }

        //
        // POST: 

        [HttpPost]
        public ActionResult PartTime(PartTimeEmployee PTemployee, string CompList, string name, string last, string day, string month, string year, string sin)
        {
            db = new EMSEntities12();
            EmployeeType ETemployee = new EmployeeType();
            ViewBag.Employees = GetOptions();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();

            Employee employee = new Employee();
            employee.FirstName = name;
            employee.LastName = last;
            int Year = 0, Month = 0, Day = 0;

            Int32.TryParse(year, out Year);
            Int32.TryParse(day, out Day);
            Int32.TryParse(month, out Month);

            employee.DateOfBirth = new DateTime(Year, Month, Day);
            employee.SIN_BN = sin;
            employee.Completed = false;

            
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    PTemployee.EmployedWith2Id = mycmp.CompanyId;
                }
            }

            ETemployee.CompanyId = PTemployee.EmployedWith2Id;
            ETemployee.EmployeeType1 = "Part Time";
            ETemployee.DateofHire = PTemployee.DateOfHire;
            PTemployee.Employee = employee;
            if (CompList == "")
            {
                ModelState.AddModelError("comp", "This field is Required.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(employee.DateOfBirth, PTemployee.DateOfHire, 16))
            {
                ModelState.AddModelError("DOH", "A part time employee must be at least 16 years old.");
            }
            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                EMSPSSUtilities.AuditNewBaseEmployee(employee, User.Identity.Name);
                PTemployee.EmployeeRef2Id = employee.EmployeeId;
                ETemployee.EmployeeId = PTemployee.EmployeeRef2Id;
                EMSPSSUtilities.CompareFields("", PTemployee.DateOfHire.ToString(), 11, 1, PTemployee.EmployeeRef2Id, User.Identity.Name);

                db.PartTimeEmployees.Add(PTemployee);
                db.EmployeeTypes.Add(ETemployee);
                db.SaveChanges();
                return RedirectToAction("SearchIndex", "Home", new { FirstName = PTemployee.Employee.FirstName, LastName = PTemployee.Employee.LastName, SINBN = PTemployee.Employee.SIN_BN });
                
            }

            return View(PTemployee);
        }


        public ActionResult SearchIndex(string FirstName, string LastName)
        {
            db = new EMSEntities12();
            var GenreLst = new List<string>();

            var GenreQry = from d in db.Employees
                           orderby d.FirstName
                           select d.FirstName;
            GenreLst.AddRange(GenreQry.Distinct());

            var employeelst = from m in db.Employees
                              select m;

            if (!String.IsNullOrEmpty(FirstName))
            {
                employeelst = employeelst.Where(s => s.FirstName.Contains(FirstName));
            }

            if (string.IsNullOrEmpty(LastName))
                return View(employeelst);
            else
            {
                return View(employeelst.Where(x => x.LastName == LastName));
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ContractCreation()
        {
            ContractEmployee ce = new ContractEmployee();
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            return View(ce);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ContractCreation(ContractEmployee ce, string CompList)
        {
            db = new EMSEntities12();
            if (CompList == "")
            {
                ModelState.AddModelError("comp", "This field is Required.");
            }
            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            Employee e = ce.Employee;
            //e.EmployeeId = null;
            e.Completed = true;
            String sin = ce.Employee.SIN_BN;
            EMSPSSUtilities.SINValid(ref sin);
            ce.Employee.SIN_BN = sin;
            if (!EMSPSSUtilities.VerifySIN(ce.Employee.SIN_BN))
            {
                ModelState.AddModelError("SIN_BN", ce.Employee.SIN_BN + " is not a valid BN.");
            }
            if (!EMSPSSUtilities.VerifyBusinessNum(ce.Employee.SIN_BN, ce.Employee.DateOfBirth.ToString("yyyy-MM-dd")))
            {
                ModelState.AddModelError("SIN_BN", "The first two digits of a BN must match the last two digits of their date of incorporation.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(ce.Employee.DateOfBirth, ce.ContractStartDate))
            {
                ModelState.AddModelError("CSD", "Contract Start date must be in the future from date of incorporation.");
            }
            if (!EMSPSSUtilities.DateIsElapsed(ce.ContractStartDate, ce.ContractStopDate))
            {
                ModelState.AddModelError("CSD2", "Contract Stop date must be in the future from Contract Start date.");
            }
            if (ModelState.IsValid)
            {
                db.Employees.Add(e);
                db.SaveChanges();

                foreach (Company mycmp in db.Companies)
                {
                    if (mycmp.CompanyName == CompList)
                    {
                        ce.EmployedWith4Id = mycmp.CompanyId;
                    }
                }
                ce.EmployeeRef4Id = e.EmployeeId;
                EmployeeType et = new EmployeeType();
                et.EmployeeId = e.EmployeeId;
                et.CompanyId = ce.EmployedWith4Id;
                et.DateofHire = ce.ContractStartDate;
                et.EmployeeType1 = "Contract";
                if (ce.ReasonForLeaving4Id == 0)
                {
                    ce.ReasonForLeaving4Id = null;
                }
                if (ModelState.IsValid)
                {
                    EMSPSSUtilities.AuditNewBaseEmployee(ce.Employee, User.Identity.Name);
                    EMSPSSUtilities.AuditNewContractEmployee(ce, User.Identity.Name);
                    db.ContractEmployees.Add(ce);
                    db.EmployeeTypes.Add(et);
                    db.SaveChanges();
                    return RedirectToAction("SearchIndex", "Home", new { FirstName = ce.Employee.FirstName, LastName = ce.Employee.LastName, SINBN = ce.Employee.SIN_BN });
                }


                return View(ce);
            }

            return View(ce);

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}