using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using pt_2b.Models;

namespace pt_2b.Controllers
{
    public class OrganisationController : Controller
    {
        private DataBase db = new DataBase();

        // GET: Organisation
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            return View(db.Organisations.ToList());
        }

        // GET: Organisation/Details/5
        [Authorize(Roles = "admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = db.Organisations.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            return View(organisation);
        }

        // GET: Organisation/Create
        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Organisation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Create([Bind(Include = "id,name")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                db.Organisations.Add(organisation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(organisation);
        }

        // GET: Organisation/Edit/5
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = db.Organisations.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            return View(organisation);
        }

        // POST: Organisation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Edit([Bind(Include = "id,name")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(organisation).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect("/organisation/Details/" + organisation.id.ToString());
            }
            return View(organisation);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Список пользователей организации
        // GET: Users
        [Authorize(Roles = "admin")]
        public ActionResult OrganisationUsers()
        {
            int orgId = Int32.Parse(Request.QueryString["orgId"]);
            Organisation org = db.Organisations.Where(o => o.id == orgId).Single();
            ViewData.Add("org", org);

            string query = @"SELECT *
                            FROM Users u
                            WHERE u.id IN (SELECT uo.userId FROM UsersOrganisations uo WHERE uo.organisationId = @orgId)
                            ORDER BY u.surname, u.name, u.patronim ASC".Replace("@orgId", Request.QueryString["orgId"]);
            
            return View(db.Database.SqlQuery<User>(query).ToList());
        }
    }
}
