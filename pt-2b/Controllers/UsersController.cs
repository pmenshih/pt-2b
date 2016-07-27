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
    public class UsersController : Controller
    {
        private DataBase db = new DataBase();

        // GET: Users
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            return View(db.User.ToList());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            if (Request.QueryString["orgId"] != null)
            {
                int orgId = Int32.Parse(Request.QueryString["orgId"]);
                Organisation org = db.Organisations.Where(o => o.id == orgId).Single();
                ViewData.Add("org", org);
            }
            
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Create([Bind(Include = "id,name,patronim,surname,email,sex")] User user)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(user);
                db.SaveChanges();

                if (Request.QueryString["orgId"] != null)
                {
                    UsersOrganisations uo = new UsersOrganisations();
                    uo.dateCreate = DateTime.Now;
                    uo.organisationId = Int32.Parse(Request.QueryString["orgId"]);
                    uo.userId = user.id;
                    db.UsersOrganisations.Add(uo);
                    db.SaveChanges();
                    return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);

                    /*
                    int orgId = Int32.Parse(Request.QueryString["orgId"]);
                    Organisation org = db.Organisations.Where(o => o.id == orgId).Single();
                    ViewData.Add("org", org);*/
                }
                else return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Edit([Bind(Include = "id,name,patronim,surname,email,sex")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                if (Request.QueryString["orgId"] != null)
                {
                    return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);
                }
                else return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.User.Find(id);
            db.User.Remove(user);
            db.UsersOrganisations.RemoveRange(db.UsersOrganisations.Where(x => x.userId == id));
            db.THSUsers.RemoveRange(db.THSUsers.Where(x => x.userId == id));
            db.SaveChanges();
            if (Request.QueryString["orgId"] != null)
            {
                return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);
            }
            else return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
