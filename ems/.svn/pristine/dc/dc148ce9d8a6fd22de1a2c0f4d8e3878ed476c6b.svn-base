using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompanyController : Controller
    {
        private EMSEntities12 db = new EMSEntities12();

        //
        // GET: /Company/

        public ViewResult Index()
        {
            return View(db.Companies.ToList());
        }

        public ActionResult SearchIndex(string companyName)
        {
            db = new EMSEntities12();
            var GenreLst = new List<string>();

            var GenreQry = from d in db.Companies
                           orderby d.CompanyName
                           select d.CompanyName;

            GenreLst.AddRange(GenreQry.Distinct());

            var companylst = from m in db.Companies
                              select m;

            if (!String.IsNullOrEmpty(companyName))
            {
                companylst = companylst.Where(s => s.CompanyName.Contains(companyName));
            }
            return View(companylst);
        }

        //
        // GET: /Company/Details/5

        public ViewResult Details(int id)
        {
            Company company = db.Companies.Find(id);
            return View(company);
        }

        //
        // GET: /Company/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Company/Create

        [HttpPost]
        public ActionResult Create(Company company)
        {
            db = new EMSEntities12();
            if (ModelState.IsValid)
            {
                List<Company> companyList = db.Companies.ToList();
                bool companyExists = false;

                for (int x = 0; x < companyList.Count; ++x)
                {
                    if (companyList[x].CompanyName == company.CompanyName)
                    {
                        companyExists = true;
                    }
                }
                if (!companyExists && company.CompanyName != null)
                {
                        company.EnrolledSince = DateTime.Now;
                        db.Companies.Add(company);
                        db.SaveChanges();
                }
                return RedirectToAction("Index");  
            }

            return View(company);
        }
        
        //
        // GET: /Company/Edit/5
 
        public ActionResult Edit(int id)
        {
            db = new EMSEntities12();
            Company company = db.Companies.Find(id);
            return View(company);
        }

        //
        // POST: /Company/Edit/5

        [HttpPost]
        public ActionResult Edit(Company company)
        {
            db = new EMSEntities12();
            if (ModelState.IsValid)
            {
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}