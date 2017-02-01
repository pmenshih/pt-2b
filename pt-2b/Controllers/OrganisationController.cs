using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using pt_2b.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Text;
using System.Text.RegularExpressions;

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
                            FROM AspNetUsers u
                            WHERE u.id IN (SELECT uo.userId FROM UsersOrganisations uo WHERE uo.organisationId = @orgId)
                            ORDER BY u.Surname, u.Name, u.Patronim ASC".Replace("@orgId", Request.QueryString["orgId"]);

            return View(db.Database.SqlQuery<AspNetUser>(query).ToList());
        }

        [Authorize(Roles = "admin")]
        public ActionResult THSFormsOwners()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult THSFormsStat()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult UsersDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("UsersDelete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult UsersDeleteConfirmed(string id)
        {
            AspNetUser user = db.AspNetUsers.Find(id);
            db.AspNetUsers.Remove(user);
            db.UsersOrganisations.RemoveRange(db.UsersOrganisations.Where(x => x.userId == id));
            db.THSUsers.RemoveRange(db.THSUsers.Where(x => x.userId == id));
            db.SaveChanges();
            if (Request.QueryString["orgId"] != null)
            {
                return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);
            }
            else return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin")]
        public ActionResult UsersCreate()
        {
            if (Request.QueryString["orgId"] != null)
            {
                int orgId = Int32.Parse(Request.QueryString["orgId"]);
                Organisation org = db.Organisations.Where(o => o.id == orgId).Single();
                ViewData.Add("org", org);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult UsersCreate([Bind(Include = "id,name,patronim,surname,email,sex,phonenumber")] AspNetUser user)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(user.PhoneNumber) && db.AspNetUsers.Where(x => x.PhoneNumber == user.PhoneNumber).Count() > 0)
                {
                    return View(user);
                }

                var newUser = new ApplicationUser { UserName = user.Email, Email = user.Email };
                newUser.Surname = user.Surname;
                newUser.Patronim = user.Patronim;
                newUser.Name = user.Name;
                newUser.PhoneNumber = user.PhoneNumber;
                newUser.Sex = user.Sex;
                var result = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Create(newUser, GeneratePassword());

                if (Request.QueryString["orgId"] != null)
                {
                    UsersOrganisations uo = new UsersOrganisations();
                    uo.dateCreate = DateTime.Now;
                    uo.organisationId = Int32.Parse(Request.QueryString["orgId"]);
                    uo.userId = newUser.Id;
                    db.UsersOrganisations.Add(uo);
                    db.SaveChanges();
                    return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);
                }
                else return RedirectToAction("Index");
            }

            return View(user);
        }

        public string GeneratePassword()
        {
            string chars = "1234567890";
            string code = "";
            Random rnd = new Random();
            for (int i = 0; i < 6; i++)
            {
                int next = rnd.Next(0, 10);
                code += chars.Substring(next, 1);
            }

            return code;
        }

        [Authorize(Roles = "admin")]
        public ActionResult UsersEdit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult UsersEdit([Bind(Include = "id,name,patronim,surname,email,sex,phonenumber")] AspNetUser user)
        {
            if (ModelState.IsValid)
            {
                if (user.PhoneNumber != null && user.PhoneNumber.Length > 0 && db.AspNetUsers.Where(x => x.PhoneNumber == user.PhoneNumber && x.Id != user.Id).Count() > 0)
                {
                    return View(user);
                }

                AspNetUser newUser = db.AspNetUsers.Find(user.Id);
                newUser.Surname = user.Surname;
                newUser.Name = user.Name;
                newUser.Patronim = user.Patronim;
                newUser.Email = user.Email;
                newUser.Sex = user.Sex;
                newUser.PhoneNumber = user.PhoneNumber;
                db.Entry(newUser).State = EntityState.Modified;
                db.SaveChanges();
                if (Request.QueryString["orgId"] != null)
                {
                    return Redirect("/organisation/organisationusers?orgId=" + Request.QueryString["orgId"]);
                }
                else return RedirectToAction("Index");
            }
            return View(user);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Results()
        {
            OrganisationResults or = new OrganisationResults();

            or.organisations = new SelectList(db.Organisations.ToList().OrderBy(x => x.name), "id", "name");

            return View(or);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult Results(FormCollection forms)
        {
            string csv = "";
            string orgname = "";
            int orgId = 0;
            string query = @"SELECT tu.id, tu.userId, tu.thsId, tu.thsUType, tu.code, tu.raw, tu.answered
                            FROM THSUsers tu
                            WHERE tu.answered = 1 
	                            AND tu.id > 0";
            int prevThsId = 0;
            AspNetUser target = null;
            string targetName = "";
            Form form = new Form();

            //подсчитываем количество ответов и формируем нулевую строку файла
            csv += "id;ta;n;t;\r\n";

            if (forms["orgs"] == "")
            {
                orgname = "Все";
            }
            else
            {
                orgId = Int32.Parse(forms["orgs"]);
                orgname = db.Organisations.Where(x => x.id == orgId).Select(x => x.name).Single();
                query += " AND thsId IN (SELECT tf.id FROM THSForms tf WHERE tf.organisationId = " + orgId.ToString() + ")";
            }
            query += " ORDER BY tu.thsId ASC, tu.thsUType ASC";

            int answersCount = 0;

            //здесь начинается костыль со сменой кодировки результатов из UTF-8 в WIN1251. временное решение
            Encoding srcEnc = Encoding.UTF8;
            Encoding destEnc = Encoding.GetEncoding(1251);

            foreach (THSUser ta in db.Database.SqlQuery<THSUser>(query))
            {
                if (ta.thsId != prevThsId)
                {
                    try
                    {
                        target = db.Database.SqlQuery<AspNetUser>("SELECT * FROM AspNetUsers WHERE id = (SELECT userId FROM THSUsers WHERE thsId=" + ta.thsId.ToString() + " AND thsUType=1)").Single();
                        targetName = target.Surname + " " + target.Name;
                    }
                    catch (Exception)
                    {
                        targetName = "";
                    }

                    prevThsId = ta.thsId;
                }

                //идентификатор ответа из БД
                csv += ta.id.ToString() + ";";

                //Фамилия Имя таргета
                csv += targetName + ";";

                //тип оценщика
                THSUserTypes uType = db.THSUserType.Where(x => x.id == ta.thsUType).Single();
                csv += uType.name + ";";
                //csv += ta.thsUType.ToString() + ";";

                //Фамилия Имя оценщика
                AspNetUser user = db.AspNetUsers.Where(x => x.Id == ta.userId).Single();
                csv += user.Surname + " " + user.Name + ";";

                //десериализуем тест (вместе с ответами)
                Form tBoxa = form.DeserializeFromXmlString(ta.raw);

                //перебираем вопросы
                foreach (Question q in tBoxa.questions)
                {
                    //меняем указанные символы на палочки. это нужно для корректного представления файла в Жкселях и других программ просмотра
                    string ans = q.answer.Replace(";", "|").Replace("\r", "|").Replace("\n", "|");
                    //смена кодировки
                    byte[] srcBytes = srcEnc.GetBytes(ans);
                    byte[] destBytes = Encoding.Convert(srcEnc, destEnc, srcBytes);
                    ans = destEnc.GetString(destBytes);
                    //---------------
                    //если в ответе содержится специальный разделитель, значит это жесткая альтернатива и номер ответа идёт после его текста
                    if (ans.Contains("@#@"))
                    {
                        csv += Regex.Split(ans, "@#@")[1];
                    }
                    else csv += ans;
                    csv += ";";
                }

                csv += "\r\n";
                answersCount++;
            }

            string curDT = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + " " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            string filename = String.Format("360-{0}-{1} {2}.csv", orgname, answersCount.ToString(), curDT);
            return File(destEnc.GetBytes(csv), "text/csv", filename);
        }
    }

    public class OrganisationResults
    {
        public SelectList organisations;
    }

    public class UserTHS
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }
}
