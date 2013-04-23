using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Utilities;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        public ActionResult Index()
        {
            db = new EMSEntities12();
            if (User.IsInRole("Admin")) // find employees that need completion
            {
                Employee employee = new Employee();
                List<Employee> emplist = new List<Employee>();

                foreach (FullTimeEmployee FtCompleted in db.FullTimeEmployees)
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
            else // find employees that need time card entries for this week
            {
                List<Employee> emplist = new List<Employee>();
                List<TimeCard> tcl = db.TimeCards.ToList();
                List<Piece> pl = db.Pieces.ToList();
                DateTime weekOf = DateTime.Now;
                while (weekOf.DayOfWeek != DayOfWeek.Monday)
                {
                    weekOf = weekOf.AddDays(-1);
                }
                Boolean Found = false;
                foreach (FullTimeEmployee ft in db.FullTimeEmployees)
                {
                    if (ft.Employee.Active && ft.Employee.Completed)
                    {
                        foreach (TimeCard tc in tcl)
                        {
                            if (tc.EmployeeRef5Id == ft.EmployeeRefId && tc.WeekOf.Day == weekOf.Day && tc.WeekOf.Month == weekOf.Month && tc.WeekOf.Year == weekOf.Year)
                            {
                                Found = true;
                                if (tc.Mon == null || tc.Sat == null || tc.Sun == null || tc.Thu == null || tc.Tue == null || tc.Wed == null || tc.Fri == null)
                                {
                                    emplist.Add(ft.Employee);
                                }
                            }
                        }
                        if (!Found)
                        {
                            emplist.Add(ft.Employee);
                            
                        }
                        Found = false;
                    }
                }
                foreach (PartTimeEmployee pt in db.PartTimeEmployees)
                {
                    if (pt.Employee.Active && pt.Employee.Completed)
                    {
                        foreach (TimeCard tc in tcl)
                        {
                            if (tc.EmployeeRef5Id == pt.EmployeeRef2Id && tc.WeekOf.Day == weekOf.Day && tc.WeekOf.Month == weekOf.Month && tc.WeekOf.Year == weekOf.Year)
                            {
                                Found = true;
                                if (tc.Mon == null || tc.Sat == null || tc.Sun == null || tc.Thu == null || tc.Tue == null || tc.Wed == null || tc.Fri == null)
                                {
                                    emplist.Add(pt.Employee);
                                }
                            }
                        }
                        if (!Found)
                        {
                            emplist.Add(pt.Employee);
                            
                        }
                        Found = false;
                    }
                }
                foreach (SeasonalEmployee se in db.SeasonalEmployees)
                {
                    if (se.Employee.Active && se.Employee.Completed)
                    {
                        foreach (TimeCard tc in tcl)
                        {
                            if (tc.EmployeeRef5Id == se.EmployeeRef3Id && tc.WeekOf.Day == weekOf.Day && tc.WeekOf.Month == weekOf.Month && tc.WeekOf.Year == weekOf.Year)
                            {
                                Found = true;
                                if (tc.Mon == null || tc.Sat == null || tc.Sun == null || tc.Thu == null || tc.Tue == null || tc.Wed == null || tc.Fri == null)
                                {
                                    emplist.Add(se.Employee);
                                }
                            }
                        }
                        if (!Found)
                        {
                            emplist.Add(se.Employee);
                            
                        }
                        Found = false;
                    }
                }

                return View(emplist);
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult SearchIndex(string FirstName2, string LastName2, string SINBN2)
        {
            db = new EMSEntities12();
            EMSPSSUtilities.UpdateEmployeeStatuses();
            var GenreLst = new List<string>();

            var GenreQry = from d in db.Employees
                           orderby d.FirstName
                           select d.FirstName;

            GenreLst.AddRange(GenreQry.Distinct());

            var employeelst = from m in db.Employees
                              select m;

            if (!String.IsNullOrEmpty(FirstName2))
            {
                employeelst = employeelst.Where(s => s.FirstName.Contains(FirstName2));
            }

            if (!String.IsNullOrEmpty(LastName2))
            {
                employeelst = (employeelst.Where(x => x.LastName.Contains(LastName2)));
            }

            if (!String.IsNullOrEmpty(SINBN2))
            {
                employeelst = (employeelst.Where(x => x.SIN_BN.Contains(SINBN2)));
            }
            List<Employee> le = employeelst.ToList();
            //if user is not an admin remove contact employees
            if (!User.IsInRole("Admin"))
            {
                for (int c = 0; c < le.Count; c++)
                {
                    if (le[c].ContractEmployees.Count > 0 || !le[c].Active)
                    {
                        le.Remove(le[c]);
                        c--;
                    }
                }
            }

            return View(le);
        }
    }
}
