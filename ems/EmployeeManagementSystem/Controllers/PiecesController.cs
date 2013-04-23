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
    public class PiecesController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        //
        // GET: /TimeCard/

        public ActionResult Index(int EmployeeId)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            List<Piece> list = new List<Piece>();
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

            foreach (Piece curr in db.Pieces)
            {
                if ((curr.EmployeeRef6Id == EmployeeId) && (curr.EmployedWith6Id == CompanyId))
                {
                    list.Add(curr);
                }
            }

            return View(list);
        }

        public ActionResult Details(int EmployeeId, DateTime weekOf)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            // test if weekOf is a monday, if not, go to employee's timecard index
            if (weekOf.Date.DayOfWeek != DayOfWeek.Monday)
            {
                return RedirectToAction("Index", "Pieces", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
            }

            foreach (Piece tmp in db.Pieces)
            {
                if ((tmp.EmployeeRef6Id == EmployeeId) && (tmp.EmployedWith6Id == CompanyId) && (tmp.WeekOf == weekOf))
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

            return RedirectToAction("Index", "Pieces", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
        }

        public ActionResult Edit(int EmployeeId, DateTime? piecesDate)
        {
            db = new EMSEntities12();
            int CompanyId = EMSPSSUtilities.GetCompanyId(EmployeeId);
            if (piecesDate != null)
            {
                DateTime weekOf = piecesDate.Value;
                while (weekOf.DayOfWeek != DayOfWeek.Monday)
                {
                    weekOf = weekOf.AddDays(-1);
                }

                foreach (Piece tmp in db.Pieces)
                {
                    if ((tmp.EmployeeRef6Id == EmployeeId) && (tmp.EmployedWith6Id == CompanyId) && (tmp.WeekOf == weekOf))
                    {
                        return View(tmp);
                    }
                }
                // if it reaches here, the timecard was not found
                // create and add it before proceeding

                Piece newPiece = new Piece();
                newPiece.EmployedWith6Id = CompanyId;
                newPiece.EmployeeRef6Id = EmployeeId;
                newPiece.WeekOf = weekOf;
                newPiece.Mon = null;
                newPiece.Tue = null;
                newPiece.Wed = null;
                newPiece.Thu = null;
                newPiece.Fri = null;
                newPiece.Sat = null;
                newPiece.Sun = null;

                db.Pieces.Add(newPiece);
                db.SaveChanges();

                return View(newPiece);
            }
            return RedirectToAction("Index", "Pieces", new { EmployeeId = EmployeeId, CompanyId = CompanyId });
        }

        [HttpPost]
        public ActionResult Edit(Piece piece)
        {
            db = new EMSEntities12();
            if (ModelState.IsValid)
            {
                db.Entry(piece).State = EntityState.Modified;
                db.SaveChanges();
                foreach (Employee emp in db.Employees)
                {
                    if (emp.EmployeeId == piece.EmployeeRef6Id)
                    {
                        ViewBag.Name = emp.FirstName + " " + emp.LastName;
                        break;
                    }
                }
                foreach (Company cmp in db.Companies)
                {
                    if (cmp.CompanyId == piece.EmployedWith6Id)
                    {
                        ViewBag.Company = cmp.CompanyName;
                        break;
                    }
                }
                ViewBag.EmployeeId = piece.EmployeeRef6Id;
                ViewBag.CompanyId = piece.EmployedWith6Id;
                return RedirectToAction("Index", "Pieces", new { EmployeeId = piece.EmployeeRef6Id, CompanyId = piece.EmployedWith6Id });
            }
            return View(piece);
        }

    }
}
