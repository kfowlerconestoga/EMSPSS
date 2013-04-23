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
    public class ReportsController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();


        public ActionResult ReportsHome()
        {
            Company company = new Company();

            ViewBag.CompName = EMSPSSUtilities.GetCompList();
            return View();
        }

        [HttpPost]
        public ActionResult ReportsHome(string ReportType, string CompList)
        {
            db = new EMSEntities12();
            EMSPSSUtilities.UpdateEmployeeStatuses();
            if (ReportType == "Seniority")
            {
                return RedirectToAction("SeniorityReport", new { CompList = CompList });
            }
            else if (ReportType == "Hours Worked")
            {
                return RedirectToAction("HoursWorkedReport", new { CompList = CompList });
            }
            else if (ReportType == "Inactive Employee Report")
            {
                return RedirectToAction("InactiveEmployeeReport", new { CompList = CompList });
            }
            else if (ReportType == "Payroll Report")
            {
                return RedirectToAction("PayrollReport", new { CompList = CompList });
            }
            else
            {
                return RedirectToAction("ActiveEmployeeReport", new { CompList = CompList });
            }

            //return View();
        }

        // GET: /Reports/

        public ActionResult SeniorityReport(string CompList)
        {
            db = new EMSEntities12();
            Company MyComp = new Company();
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    MyComp.CompanyId = mycmp.CompanyId;
                }
            }
            //Dictionary<EmployeeType, String> emplist = new Dictionary<EmployeeType,String>();
            Dictionary<EmployeeType, String> emplist2 = new Dictionary<EmployeeType, String>();
            List<EmployeeType> emplist = new List<EmployeeType>();
            List<String> strlst = new List<String>();
            foreach (EmployeeType emptype in db.EmployeeTypes)
            {
                if (emptype.CompanyId == MyComp.CompanyId && emptype.Employee.Completed && emptype.Employee.Active)
                {
                    emplist.Add(emptype);
                    ViewBag.CompanyName = emptype.Company.CompanyName;
                    // strlst.Add(ParseDays(emptype.DateofHire));

                }

            }

            foreach (EmployeeType emptypes in emplist.OrderBy(x => x.DateofHire))
            {
                emplist2.Add(emptypes, ParseDays(emptypes.DateofHire));
            }

            //ViewBag.EmplList = strlst;
            return View(emplist2);
        }


        public ActionResult HoursWorkedReport(string CompList)
        {
            ViewBag.Company = CompList;

            Dictionary<EmployeeType, decimal?> emplist2 = new Dictionary<EmployeeType, decimal?>();


            return View(emplist2);
        }

        private string ParseDays(DateTime Date)
        {
            string NewDate;
            Int32 offset;
            offset = DateTime.Now.Year - Date.Year;
            NewDate = offset.ToString() + " Years";

            if (offset == 0)
            {
                NewDate = string.Empty;
                offset = DateTime.Now.Month - Date.Month;
                NewDate = offset.ToString() + " Months";
            }
            else if (offset == 0)
            {
                NewDate = string.Empty;
                int days = ((TimeSpan)(DateTime.Now - Date)).Days;
                NewDate = days.ToString() + " Days";
            }

            return NewDate;
        }


        private decimal? TotalHours(TimeCard Hours)
        {
            decimal? total;

            total = Hours.Mon.GetValueOrDefault(0) + Hours.Tue.GetValueOrDefault(0) + Hours.Wed.GetValueOrDefault(0) + Hours.Thu.GetValueOrDefault(0) + Hours.Fri.GetValueOrDefault(0) + Hours.Sat.GetValueOrDefault(0) + Hours.Sun.GetValueOrDefault(0);
            return total;
        }


        public ActionResult HoursIndex(Nullable<DateTime> DateTimeQuery, string CompList)
        {
            db = new EMSEntities12();
            //if (DateTimeQuery != null)
            //{

            //    return View("HoursWorkedReport" );
            //}
            DateTime timeofweek = DateTime.Now ;
            Company MyComp = new Company();
            EmployeeType MyEmp = new EmployeeType();
            TimeCard TimeCardEnt = new TimeCard();
            Dictionary<EmployeeType, decimal?> emplist2 = new Dictionary<EmployeeType, decimal?>();
            List<TimeCard> tmcard = new List<TimeCard>();
            if (DateTimeQuery != null)
            {
                ViewBag.ThisCompany = CompList;
                timeofweek = (DateTime)DateTimeQuery;
                
             
                foreach (Company mycmp in db.Companies)
                {
                    if (mycmp.CompanyName == CompList)
                    {
                        MyComp.CompanyId = mycmp.CompanyId;

                    }


                }

            
            while (timeofweek.DayOfWeek != DayOfWeek.Sunday)
                {
                    timeofweek = timeofweek.AddDays(+1);
                }
                ViewBag.DateWeek = timeofweek;

            }
            List<EmployeeType> emplist = new List<EmployeeType>();


            foreach (EmployeeType EmpTime in db.EmployeeTypes)
            {
                if (EmpTime.CompanyId == MyComp.CompanyId)
                {
                    if (EmpTime.Employee.Completed && EmpTime.Employee.Active)
                    {
                        if (EmpTime.EmployeeType1 != "Contract  ")
                        {

                            emplist.Add(EmpTime);

                        }
                    }
                }
            }

            foreach (TimeCard TimecrdEnt in db.TimeCards)
            {

                if (TimecrdEnt.EmployedWith5Id == MyComp.CompanyId)
                {
                    tmcard.Add(TimecrdEnt);

                }


            }


            foreach (EmployeeType EMP in emplist)
            {

                foreach (TimeCard tmcrd in tmcard)
                {

                    if (EMP.EmployeeId == tmcrd.EmployeeRef5Id)
                    {
                        if (DateTimeQuery == tmcrd.WeekOf)
                        {

                            emplist2.Add(EMP, TotalHours(tmcrd));
                        }

                    }
                }
            }


            return View("HoursWorkedReport", emplist2);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ActiveEmployeeReport(string CompList)
        {
            db = new EMSEntities12();
            ViewBag.Company = CompList;
            int compId = 0;
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    compId = mycmp.CompanyId;
                }
            }

            Dictionary<FullTimeEmployee, float> ftEmpList = new Dictionary<FullTimeEmployee, float>();
            Dictionary<PartTimeEmployee, float> ptEmpList = new Dictionary<PartTimeEmployee, float>();
            Dictionary<SeasonalEmployee, float> seEmpList = new Dictionary<SeasonalEmployee, float>();
            List<ContractEmployee> conEmpList = new List<ContractEmployee>();
            foreach (EmployeeType et in db.EmployeeTypes)
            {
                if (et.CompanyId == compId)
                {
                    if (et.EmployeeType1 != "Contract  ")
                    {
                        if (et.Employee.FullTimeEmployees.Count > 0)
                        {
                            FullTimeEmployee fulltimer = EMSPSSUtilities.GetFullTimer(et.EmployeeId);
                            if ((fulltimer.DateOfTermination > DateTime.Now || fulltimer.DateOfTermination == null) && fulltimer.Employee.Completed)
                            {
                                ftEmpList.Add(fulltimer, CalculateAvgHourWorked(et.Employee.EmployeeId));
                            }
                        }
                        else if (et.Employee.PartTimeEmployees.Count > 0)
                        {
                            PartTimeEmployee parttimer = EMSPSSUtilities.GetPartTimer(et.EmployeeId);
                            if ((parttimer.DateOfTermination > DateTime.Now || parttimer.DateOfTermination == null) && parttimer.Employee.Completed)
                            {
                                ptEmpList.Add(parttimer, CalculateAvgHourWorked(et.Employee.EmployeeId));
                            }
                        }
                        else if (et.Employee.SeasonalEmployees.Count > 0)
                        {

                            SeasonalEmployee sea = EMSPSSUtilities.GetSeasonal(et.EmployeeId);
                            DateTime lowRange = DateTime.Now;
                            DateTime highRange = DateTime.Now;
                            if (sea.Season.Season1 == "Winter")
                            {
                                lowRange = new DateTime(DateTime.Now.Year - 1, 12, 1);
                                highRange = new DateTime(DateTime.Now.Year, 4, 30);
                            }
                            else if (sea.Season.Season1 == "Spring")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 5, 1);
                                highRange = new DateTime(DateTime.Now.Year, 6, 30);
                            }
                            else if (sea.Season.Season1 == "Summer")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 7, 1);
                                highRange = new DateTime(DateTime.Now.Year, 8, 31);
                            }
                            else if (sea.Season.Season1 == "Fall")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 9, 1);
                                highRange = new DateTime(DateTime.Now.Year, 11, 30);
                            }
                            if (sea.SeasonYear >= lowRange && sea.SeasonYear <= highRange && sea.Employee.Completed)
                            {
                                seEmpList.Add(sea, CalculateAvgHourWorked(et.Employee.EmployeeId));
                            }
                        }
                    }
                    else
                    {
                        ContractEmployee con = EMSPSSUtilities.GetContract(et.EmployeeId);
                        if (con.ContractStopDate > DateTime.Now && con.ContractStartDate < DateTime.Now)
                        {
                            conEmpList.Add(con);
                        }
                    }
                }
            }
            ViewBag.ftlist = ftEmpList;
            ViewBag.ptlist = ptEmpList;
            ViewBag.sealist = seEmpList;
            ViewBag.conlist = conEmpList;
            return View();
        }

        private float CalculateAvgHourWorked(int id)
        {
            float avgHours = 0;
            int i = 0;
            foreach (TimeCard tc in db.TimeCards)
            {
                if (id == tc.EmployeeRef5Id)
                {
                    avgHours += addHours(tc);
                    i++;
                }
            }
            if (i != 0)
            {
                avgHours = avgHours / i;
            }

            return avgHours;
        }

        private float addHours(TimeCard tc)
        {
            float hours = 0;
            if (tc.Mon != null)
            {
                hours += (float)tc.Mon;
            }
            if (tc.Tue != null)
            {
                hours += (float)tc.Tue;
            }
            if (tc.Wed != null)
            {
                hours += (float)tc.Wed;
            }
            if (tc.Thu != null)
            {
                hours += (float)tc.Thu;
            }
            if (tc.Fri != null)
            {
                hours += (float)tc.Fri;
            }
            if (tc.Sat != null)
            {
                hours += (float)tc.Sat;
            }
            if (tc.Sun != null)
            {
                hours += (float)tc.Sun;
            }
            return hours;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult InactiveEmployeeReport(string CompList)
        {
            db = new EMSEntities12();
            ViewBag.Company = CompList;
            int compId = 0;
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {

                    compId = mycmp.CompanyId;
                }
            }

            List<FullTimeEmployee> ftEmpList = new List<FullTimeEmployee>();
            List<PartTimeEmployee> ptEmpList = new List<PartTimeEmployee>();
            List<SeasonalEmployee> seEmpList = new List<SeasonalEmployee>();
            List<ContractEmployee> conEmpList = new List<ContractEmployee>();
            foreach (EmployeeType et in db.EmployeeTypes)
            {
                if (et.CompanyId == compId)
                {
                    if (et.EmployeeType1 != "Contract  ")
                    {
                        if (et.Employee.FullTimeEmployees.Count > 0)
                        {
                            FullTimeEmployee fulltimer = EMSPSSUtilities.GetFullTimer(et.EmployeeId);
                            if ((fulltimer.DateOfTermination < DateTime.Now && fulltimer.DateOfTermination != null) && fulltimer.Employee.Completed)
                            {
                                if (fulltimer.ReasonForLeavingId == null)
                                {
                                    ReasonForLeaving re = new ReasonForLeaving();
                                    re.ReasonForLeaving1 = "--";
                                    fulltimer.ReasonForLeaving = re;
                                }
                                ftEmpList.Add(fulltimer);
                            }
                        }
                        else if (et.Employee.PartTimeEmployees.Count > 0)
                        {
                            PartTimeEmployee parttimer = EMSPSSUtilities.GetPartTimer(et.EmployeeId);
                            if ((parttimer.DateOfTermination < DateTime.Now && parttimer.DateOfTermination != null) && parttimer.Employee.Completed)
                            {
                                if (parttimer.ReasonForLeaving2Id == null)
                                {
                                    ReasonForLeaving re = new ReasonForLeaving();
                                    re.ReasonForLeaving1 = "--";
                                    parttimer.ReasonForLeaving = re;
                                }
                                ptEmpList.Add(parttimer);
                            }
                        }
                        else if (et.Employee.SeasonalEmployees.Count > 0)
                        {
                            SeasonalEmployee sea = EMSPSSUtilities.GetSeasonal(et.EmployeeId);
                            DateTime lowRange = DateTime.Now;
                            DateTime highRange = DateTime.Now;
                            if (sea.Season.Season1 == "Winter")
                            {
                                lowRange = new DateTime(DateTime.Now.Year - 1, 12, 1);
                                highRange = new DateTime(DateTime.Now.Year, 4, 30);
                            }
                            else if (sea.Season.Season1 == "Spring")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 5, 1);
                                highRange = new DateTime(DateTime.Now.Year, 6, 30);
                            }
                            else if (sea.Season.Season1 == "Summer")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 7, 1);
                                highRange = new DateTime(DateTime.Now.Year, 8, 31);
                            }
                            else if (sea.Season.Season1 == "Fall")
                            {
                                lowRange = new DateTime(DateTime.Now.Year, 9, 1);
                                highRange = new DateTime(DateTime.Now.Year, 11, 30);
                            }
                            if (DateTime.Now < lowRange || DateTime.Now > highRange && sea.Employee.Completed)
                            {
                                if (sea.ReasonForLeaving3Id == null)
                                {
                                    ReasonForLeaving re = new ReasonForLeaving();
                                    re.ReasonForLeaving1 = "--";
                                    sea.ReasonForLeaving = re;
                                }
                                seEmpList.Add(sea);
                            }
                        }
                    }
                    else
                    {
                        ContractEmployee con = EMSPSSUtilities.GetContract(et.EmployeeId);
                        if (con.ContractStopDate < DateTime.Now || con.ContractStartDate > DateTime.Now)
                        {
                            if (con.ReasonForLeaving4Id == null)
                            {
                                ReasonForLeaving re = new ReasonForLeaving();
                                re.ReasonForLeaving1 = "--";
                                con.ReasonForLeaving = re;
                            }
                            conEmpList.Add(con);
                        }
                    }

                }
            }
            ViewBag.ftlist = ftEmpList;
            ViewBag.ptlist = ptEmpList;
            ViewBag.sealist = seEmpList;
            ViewBag.conlist = conEmpList;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PayrollReport(string CompList)
        {
            db = new EMSEntities12();
            ViewBag.Company = CompList;
            int compId = 0;
            foreach (Company mycmp in db.Companies)
            {
                if (mycmp.CompanyName == CompList)
                {
                    compId = mycmp.CompanyId;
                }
            }

            List<FullTimeEmployee> ftEmpList = new List<FullTimeEmployee>();
            List<String> ftNotes = new List<String>();
            List<float> ftHours = new List<float>();
            List<float> ftPay = new List<float>();

            List<PartTimeEmployee> ptEmpList = new List<PartTimeEmployee>();
            List<String> ptNotes = new List<String>();
            List<float> ptHours = new List<float>();
            List<float> ptPay = new List<float>();

            List<SeasonalEmployee> seaEmpList = new List<SeasonalEmployee>();
            List<String> seaNotes = new List<String>();
            List<float> seaHours = new List<float>();
            List<float> seaPay = new List<float>();

            List<float> pieceTally = new List<float>();

            List<ContractEmployee> conEmpList = new List<ContractEmployee>();
            List<String> conNotes = new List<String>();


            foreach (EmployeeType et in db.EmployeeTypes)
            {
                if (et.CompanyId == compId)
                {
                    if (et.Employee.Active && et.Employee.Completed)
                    {


                        if (et.EmployeeType1 != "Contract  ")
                        {
                            if (et.Employee.FullTimeEmployees.Count > 0)
                            {
                                string comment = "";
                                FullTimeEmployee fulltimer = EMSPSSUtilities.GetFullTimer(et.EmployeeId);
                                if (fulltimer.Employee.Completed)
                                {
                                    if (fulltimer.DateOfTermination == null || fulltimer.DateOfTermination > DateTime.Now)
                                    {
                                        float hoursWorked = addHours(GetTimeCardEntry(fulltimer.EmployeeRefId));
                                        if (hoursWorked < 40)
                                        {
                                            comment = "Not full work week";
                                        }
                                        ftEmpList.Add(fulltimer);
                                        ftNotes.Add(comment);
                                        ftHours.Add(hoursWorked);
                                        ftPay.Add(((float)fulltimer.Salary / 52));
                                    }
                                }
                            }
                            else if (et.Employee.PartTimeEmployees.Count > 0)
                            {
                                string comment = "";
                                PartTimeEmployee parttimer = EMSPSSUtilities.GetPartTimer(et.EmployeeId);
                                if (parttimer.Employee.Completed)
                                {
                                    if (parttimer.DateOfTermination == null || parttimer.DateOfTermination > DateTime.Now)
                                    {
                                        float hoursWorked = addHours(GetTimeCardEntry(parttimer.EmployeeRef2Id));
                                        float pay = 0;
                                        if (hoursWorked > 40)
                                        {
                                            comment = (hoursWorked - 40).ToString() + " hours of OT";
                                            float rate = (float)parttimer.HourlyRate;
                                            pay = (40 * rate) + ((hoursWorked - 40) * (rate * 1.5f));
                                        }
                                        else
                                        {
                                            pay = ((float)parttimer.HourlyRate * hoursWorked);
                                        }
                                        ptEmpList.Add(parttimer);
                                        ptNotes.Add(comment);
                                        ptHours.Add(hoursWorked);
                                        ptPay.Add(pay);
                                    }
                                }

                            }
                            else if (et.Employee.SeasonalEmployees.Count > 0)
                            {
                                SeasonalEmployee sea = EMSPSSUtilities.GetSeasonal(et.EmployeeId);
                                if (sea.Employee.Completed)
                                {
                                    if (sea.SeasonYear.Year == DateTime.Now.Year)
                                    {
                                        string comment = "";
                                        float pay = 0;
                                        float hoursWorked = addHours(GetTimeCardEntry(sea.EmployeeRef3Id));
                                        float pieces = TallyPieces(GetPiecesEntry(sea.EmployeeRef3Id));
                                        pieceTally.Add(pieces);
                                        pay = pieces * (float)sea.PiecePay;
                                        if (hoursWorked > 40)
                                        {
                                            pay += 150;
                                        }
                                        seaEmpList.Add(sea);
                                        seaNotes.Add(comment);
                                        seaHours.Add(hoursWorked);
                                        seaPay.Add(pay);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ContractEmployee con = EMSPSSUtilities.GetContract(et.EmployeeId);
                            TimeSpan span = con.ContractStopDate - DateTime.Now;
                            TimeSpan span2 = con.ContractStopDate - con.ContractStartDate;
                            if (span.TotalDays > 0)
                            {
                                conNotes.Add(span.TotalDays.ToString("n2") + " days remaining.");
                                con.FixedContractAmount = (con.FixedContractAmount * 7) / span2.TotalDays;
                                conEmpList.Add(con);
                            }
                        }
                    }
                }
            }
            if (pieceTally.Count > 0)
            {
                int highest = DecideMostProductive(pieceTally);
                seaNotes[highest] = "Most Productive";
            }

            ViewBag.ftlist = ftEmpList;
            ViewBag.fthours = ftHours;
            ViewBag.ftpay = ftPay;
            ViewBag.ftnotes = ftNotes;

            ViewBag.ptlist = ptEmpList;
            ViewBag.pthours = ptHours;
            ViewBag.ptpay = ptPay;
            ViewBag.ptnotes = ptNotes;

            ViewBag.sealist = seaEmpList;
            ViewBag.seahours = seaHours;
            ViewBag.seapay = seaPay;
            ViewBag.seanotes = seaNotes;

            ViewBag.conlist = conEmpList;
            ViewBag.connotes = conNotes;


            return View();
        }



        private int DecideMostProductive(List<float> pieces)
        {
            int highest = 0;
            if (pieces.Count > 0)
            {
                float current = pieces[0];

                for (int x = 1; x < pieces.Count; x++)
                {
                    if (pieces[x] > current)
                    {
                        highest = x;
                    }
                }
            }
            return highest;
        }

        private TimeCard GetTimeCardEntry(int employeeId)
        {
            TimeCard tc = new TimeCard();
            DateTime weekOf = DateTime.Now.Date;
            while (weekOf.DayOfWeek != DayOfWeek.Monday)
            {
                weekOf = weekOf.AddDays(-1);
            }

            List<TimeCard> allTCs = db.TimeCards.ToList();
            foreach (TimeCard tmp in allTCs)
            {
                if ((tmp.EmployeeRef5Id == employeeId) && (tmp.WeekOf == weekOf))
                {
                    tc = tmp;
                }
            }


            return tc;
        }

        private float CalculateThisWeeksPayFT(float hoursWorked, float salary)
        {
            float pay = 0;

            return pay;
        }

        private float TallyPieces(Piece p)
        {
            float pieces = 0;
            if (p.Mon != null)
            {
                pieces += (float)p.Mon;
            }
            if (p.Tue != null)
            {
                pieces += (float)p.Tue;
            }
            if (p.Wed != null)
            {
                pieces += (float)p.Wed;
            }
            if (p.Thu != null)
            {
                pieces += (float)p.Thu;
            }
            if (p.Fri != null)
            {
                pieces += (float)p.Fri;
            }
            if (p.Sat != null)
            {
                pieces += (float)p.Sat;
            }
            if (p.Sun != null)
            {
                pieces += (float)p.Sun;
            }
            return pieces;
        }

        private Piece GetPiecesEntry(int employeeId)
        {
            Piece pc = new Piece();
            DateTime weekOf = DateTime.Now.Date;
            while (weekOf.DayOfWeek != DayOfWeek.Monday)
            {
                weekOf = weekOf.AddDays(-1);
            }

            List<Piece> allPCs = db.Pieces.ToList();
            foreach (Piece tmp in allPCs)
            {
                if ((tmp.EmployeeRef6Id == employeeId) && (tmp.WeekOf == weekOf))
                {
                    pc = tmp;
                }
            }


            return pc;
        }
    }
}
