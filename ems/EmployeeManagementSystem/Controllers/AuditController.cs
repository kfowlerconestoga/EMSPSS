using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        //
        // GET: /Audit/
        private EMSEntities12 db = new EMSEntities12();

        public ActionResult Index()
        {
            db = new EMSEntities12();
            List<Audit> al = db.Audits.ToList();
            al = FixList(al);
            return View(al);
        }

        public ActionResult SearchIndex(Nullable<DateTime> DateTime)
        {
            db = new EMSEntities12();
            List<Audit> al = db.Audits.ToList();
            if (DateTime != null)
            {
                List<Audit> returnList = new List<Audit>();
                foreach (Audit a in al)
                {
                    if (a.DateTime > DateTime)
                    {
                        returnList.Add(a);
                    }
                }
                returnList = FixList(returnList);
                return View("Index", returnList);
            }
            al = FixList(al);
            return View("Index", al);
        }

        private List<Audit> FixList(List<Audit> al)
        {
            db = new EMSEntities12();
            for (int x = 0; x < al.Count;x++ )
            {
                if (al[x].Fieldname.Fieldname1 == "ReasonForLeavingId" || al[x].Fieldname.Fieldname1 == "ReasonForLeaving")
                {
                    al[x].Fieldname.Fieldname1 = "ReasonForLeaving";
                    if (al[x].OldValue != "")
                    {
                        Int32 id = Int32.Parse(al[x].OldValue);
                        if (id != 0)
                        {
                            al[x].OldValue = db.ReasonForLeavings.Find(id).ReasonForLeaving1;
                        }
                    }
                    if (al[x].NewValue != "")
                    {
                        Int32 id = Int32.Parse(al[x].NewValue);
                        if (id != 0)
                        {
                            al[x].NewValue = db.ReasonForLeavings.Find(id).ReasonForLeaving1;
                        }
                    }                    
                }
                else if (al[x].Fieldname.Fieldname1 == "SeasonId" || al[x].Fieldname.Fieldname1 == "Season")
                {
                    al[x].Fieldname.Fieldname1 = "Season";
                    if (al[x].OldValue != "")
                    {
                        Int32 id = Int32.Parse(al[x].OldValue);
                        if (id != 0)
                        {
                            al[x].OldValue = db.Seasons.Find(id).Season1;
                        }
                    }
                    if (al[x].NewValue != "")
                    {
                        Int32 id = Int32.Parse(al[x].NewValue);
                        if (id != 0)
                        {
                            al[x].NewValue = db.Seasons.Find(id).Season1;
                        }
                    }   
                }
            }
            return al;
        }

    }
}
