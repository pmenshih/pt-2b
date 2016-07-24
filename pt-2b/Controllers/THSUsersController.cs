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
    public class THSUsersController : Controller
    {
        private DataBase db = new DataBase();

        // GET: THSUsers
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            return View(db.THSUsers.ToList());
        }

        // GET: THSUsers/Details/5
        [Authorize(Roles = "admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSUser tHSUser = db.THSUsers.Find(id);
            if (tHSUser == null)
            {
                return HttpNotFound();
            }
            return View(tHSUser);
        }

        // GET: THSUsers/Create
        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: THSUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Create([Bind(Include = "id,userId,thsId,thsUType,code,raw,answered")] THSUser tHSUser)
        {
            if (ModelState.IsValid)
            {
                db.THSUsers.Add(tHSUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tHSUser);
        }

        // GET: THSUsers/Edit/5
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSUser tHSUser = db.THSUsers.Find(id);
            if (tHSUser == null)
            {
                return HttpNotFound();
            }
            return View(tHSUser);
        }

        // POST: THSUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Edit([Bind(Include = "id,userId,thsId,thsUType,code,raw,answered")] THSUser tHSUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tHSUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tHSUser);
        }

        // GET: THSUsers/Delete/5
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSUser tHSUser = db.THSUsers.Find(id);
            if (tHSUser == null)
            {
                return HttpNotFound();
            }
            return View(tHSUser);
        }

        // POST: THSUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            THSUser tHSUser = db.THSUsers.Find(id);
            db.THSUsers.Remove(tHSUser);
            db.SaveChanges();
            return RedirectToAction("Index");
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
