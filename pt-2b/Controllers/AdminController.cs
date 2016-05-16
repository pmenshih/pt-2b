using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

using pt_2b.Models;

namespace pt_2b.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        DataBase db = new DataBase();

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public ActionResult TestUpload(FormCollection formCollection)
        {
            HttpPostedFileBase file = Request.Files["testFile"];

            byte[] fileBytes = new byte[file.ContentLength];
            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            string testString = System.Text.Encoding.UTF8.GetString(fileBytes);

            bool uploadCanUpdate = formCollection["uploadCanUpdate"] == "1" ? true : false;

            //десериализация
            Models.Form test = new Form();
            test = test.DeserializeFromXmlString(testString);

            db.Forms.Add(test);
            db.SaveChanges();

            return Redirect("/admin");
        }
    }
}