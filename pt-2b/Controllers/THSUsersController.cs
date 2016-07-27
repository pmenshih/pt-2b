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
            if (Request.QueryString["thsId"] != null && Request.QueryString["orgId"] != null)
            {
                return Redirect("/THSForms/Details/" + Request.QueryString["thsId"] + "?orgId=" + Request.QueryString["orgId"]);
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
