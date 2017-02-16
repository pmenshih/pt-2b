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

        [Authorize(Roles = "admin")]
        public ActionResult ImportUsers(string orgId)
        {
            var model = new Models.Organisations.Views.UsersImport();
            model.orgId = orgId;
            model.uploadHistory = Organisation.GetUploadHistory(orgId);

            return View(model);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult ImportUsers(Models.Organisations.Views.UsersImport model)
        {
            model.uploadHistory = Organisation.GetUploadHistory(model.orgId);

            if (model.filename == null)
            {
                ViewData["serverError"] = Core.ErrorMessages.UploadFileNotSelect;
                return View(model);
            }

            //если не указан сепаратор
            if (String.IsNullOrEmpty(model.separator))
            {
                ViewData["serverError"] = Core.ErrorMessages.UploadFileNoSeparator;
                return View(model);
            }

            //очистим все счетчики и журналы
            model.errorLog.Clear();
            model.rowsCount = 0;
            model.rowsCorrect = 0;
            model.rowsIncorrect = 0;
            model.usersAdded = 0;
            model.usersNotAdded = 0;

            //прочитаем файл в строку
            string usersFile = Core.BLL.ReadUploadedFileToString(Request.Files["filename"]);

            //рассплитуем файл построчно
            string[] ufStrings = Regex.Split(usersFile, "\r\n");
            //в файле меньше двух строк
            if (ufStrings.Count() < 2)
            {
                ViewData["serverError"] = Core.ErrorMessages.UploadUsersFileLessTwoStrings;
                return View(model);
            }

            int scnEmail = -1;
            int scnFullname = -1;
            int scnSex = -1;
            int scnPwd = -1;

            int dcnEmail = -1;
            int dcnFullname = -1;
            int dcnSex = -1;
            int dcnPwd = -1;

            string historyFile = "";

            //разберем первую строку-заголовок
            int dataColumnCounter = 0;
            int destColumnCounter = 0;
            foreach (string s in Regex.Split(ufStrings[0], model.separator))
            {
                //узнаем индексы нужных полей
                switch (s)
                {
                    case "email":
                        scnEmail = dataColumnCounter;
                        historyFile += s + model.separator;
                        dcnEmail = destColumnCounter;
                        destColumnCounter++;
                        break;
                    case "fullname":
                        scnFullname = dataColumnCounter;
                        historyFile += s + model.separator;
                        dcnFullname = destColumnCounter;
                        destColumnCounter++;
                        break;
                    case "sex":
                        scnSex = dataColumnCounter;
                        historyFile += s + model.separator;
                        dcnSex = destColumnCounter;
                        destColumnCounter++;
                        break;
                    case "pwd":
                        scnPwd = dataColumnCounter;
                        historyFile += s + model.separator;
                        dcnPwd = destColumnCounter;
                        destColumnCounter++;
                        break;
                    default:
                        break;
                }
                dataColumnCounter++;
            }

            //если в исходном файле нет поля с паролем
            //то нам нужно добавить их в конец таблицы для истории загрузок
            if (dcnPwd == -1)
            {
                historyFile += "pwd" + model.separator;
                dcnPwd = destColumnCounter;
                destColumnCounter++;
            }

            //если поля email нет, то сообщим об этом
            if (scnEmail == -1)
            {
                ViewData["serverError"] = Core.ErrorMessages.UploadUsersFileNoEmail;
                return View(model);
            }

            //начинаем собирать записи пользователей и добавлять их
            int rowsCounter = 0;
            foreach (string fileString in ufStrings)
            {
                string[] historyString = new string[destColumnCounter];

                rowsCounter++;

                //первую строку пропустим
                if (rowsCounter - 1 == 0) continue;

                //если строка пустая, тоже пропустим
                if (fileString.Length < 1) continue;

                model.rowsCount++;

                string[] columns = Regex.Split(fileString, model.separator);

                if (columns.Length < dataColumnCounter)
                {
                    model.rowsIncorrect++;
                    string a = "Количество стобцов меньше чем в заголовке";
                    string b = columns.Length.ToString() + " из " + dataColumnCounter.ToString();
                    Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                ,fileString
                                                                                ,a
                                                                                ,b);
                    model.errorLog.Add(log);
                    continue;
                }

                //проверим поля на корректность
                //почта
                string email = null;
                try { email = columns[scnEmail]; }
                catch (Exception)
                {
                    model.rowsIncorrect++;
                    continue;
                }
                Regex regex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,5})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
                Match match = regex.Match(email);
                //если адрес почты не соответствует шаблону
                if (!match.Success)
                {
                    model.rowsIncorrect++;

                    //добавим подробности в журнал ошибок
                    Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                , fileString
                                                                                , "email"
                                                                                , email);
                    model.errorLog.Add(log);

                    continue;
                }
                else if (db.AspNetUsers.Where(x => x.Email == email).Count() > 0)
                {
                    model.rowsCorrect++;
                    model.usersNotAdded++;
                    string b = "адрес уже зарегистрирован";
                    Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                , fileString
                                                                                , "email"
                                                                                , b);
                    model.errorLog.Add(log);
                    continue;
                }
                historyString[dcnEmail] = email;

                //фамилия, имя и отчество
                string fullname = null;
                if (scnFullname != -1 && columns[scnFullname] != String.Empty)
                {
                    fullname = columns[scnFullname];
                    if (fullname.Length > 70)
                    {
                        model.rowsIncorrect++;
                        Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                    , fileString
                                                                                    , "fullname"
                                                                                    , columns[scnFullname]);
                        model.errorLog.Add(log);
                        continue;
                    }
                }
                if (scnFullname != -1) historyString[dcnFullname] = fullname;

                //пол
                byte? sex = null;
                if (scnSex != -1 && columns[scnSex] != String.Empty)
                {
                    if (columns[scnSex] == "1" || columns[scnSex] == "0")
                        sex = Byte.Parse(columns[scnSex]);
                    else
                    {
                        model.rowsIncorrect++;
                        Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                    , fileString
                                                                                    , "sex"
                                                                                    , columns[scnSex]);
                        model.errorLog.Add(log);
                        continue;
                    }
                }
                if (scnSex != -1) historyString[dcnSex] = sex.ToString();

                //пароль
                string password = null;
                if (scnPwd != -1 && columns[scnPwd] != String.Empty)
                {
                    password = columns[scnPwd];
                    //минимум 1 латинская буква, минимум 1 НЕбуква, минимум 6 символов
                    regex = new Regex(@"^(.{0,}(([a-zA-Z][^a-zA-Z])|([^a-zA-Z][a-zA-Z])).{4,})|(.{1,}(([a-zA-Z][^a-zA-Z])|([^a-zA-Z][a-zA-Z])).{3,})|(.{2,}(([a-zA-Z][^a-zA-Z])|([^a-zA-Z][a-zA-Z])).{2,})|(.{3,}(([a-zA-Z][^a-zA-Z])|([^a-zA-Z][a-zA-Z])).{1,})|(.{4,}(([a-zA-Z][^a-zA-Z])|([^a-zA-Z][a-zA-Z])).{0,})$");
                    match = regex.Match(password);
                    //если пароль не соответствует шаблону
                    if (!match.Success)
                    {
                        model.rowsIncorrect++;
                        Core.UploadFailedString log = new Core.UploadFailedString(rowsCounter
                                                                                    , fileString
                                                                                    , "pwd"
                                                                                    , columns[scnPwd]);
                        model.errorLog.Add(log);
                        continue;
                    }
                }
                else
                {
                    password = Core.BLL.GenerateRandomDigitCode(6);
                }
                historyString[dcnPwd] = password;

                //если все проверки пройдены, увеличим счетчик распознанного как строки пользователей
                model.rowsCorrect++;

                email = email.ToLower();

                //добавим пользователя в БД
                var newUser = new ApplicationUser { UserName = email, Email = email };
                // если поле полного имени непустое, разберём его на составляющие
                string surname = "", name = "", patronim = "";
                if (!String.IsNullOrEmpty(fullname)) {
                    // Уберём все пробелы в конце и начале
                    fullname = fullname.Trim();
                    // Все множественные пробелы заменим на 1
                    Regex rgx = new Regex("\\s+");
                    fullname = rgx.Replace(fullname, " ");

                    string[] n = fullname.Split(' ');
                    // только фамилия
                    if (n.Length == 1)
                    {
                        surname = n[0];
                    }
                    //фамилия + имя
                    else if (n.Length == 2)
                    {
                        surname = n[0];
                        name = n[1];
                    }
                    // ФИО
                    else if (n.Length == 3) {
                        surname = n[0];
                        name = n[1];
                        patronim = n[2];
                    }
                }

                newUser.Surname = surname;
                newUser.Patronim = patronim;
                newUser.Name = name;
                newUser.Sex = sex;
                var result = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>()
                    .Create(newUser, password);
                if (result.Succeeded)
                {
                    //добавим пользователя к организации
                    UsersOrganisations uo = new UsersOrganisations();
                    uo.dateCreate = DateTime.Now;
                    uo.organisationId = Int32.Parse(model.orgId);
                    uo.userId = newUser.Id;
                    db.UsersOrganisations.Add(uo);
                    model.usersAdded++;
                }

                //сформируем строку параметров пользователя для файла в историю загрузок
                historyFile += "\r\n";
                foreach (string s in historyString)
                {
                    historyFile += s + model.separator;
                }
            }

            //если пользователи добавлены, сохраним файл историй в БД
            if (model.usersAdded > 0)
            {
                Models.Organisations.OrganisationsUsersFile oufs = new Models.Organisations.OrganisationsUsersFile();
                oufs.dateCreate = DateTime.Now;
                oufs.deleted = false;
                oufs.id = Guid.NewGuid().ToString();
                oufs.orgId = model.orgId;
                oufs.usersCount = model.usersAdded;
                oufs.usersData = historyFile;
                db.OrganisationsUsersFiles.Add(oufs);
                db.SaveChanges();
                db.Dispose();
            }

            model.uploadHistory = Organisation.GetUploadHistory(model.orgId);
            model.showResult = true;

            return View(model);
        }

        [Authorize(Roles = "admin")]
        public ActionResult MailTemplate()
        {
            var model = new pt_2b.Models.Organisations.Views.MailTemplate();
            model.research = pt_2b.Models.OrganisationsResearches.OrganisationsResearch.GetById(Int32.Parse(Request.QueryString["researchId"]));
            model.organisation = db.Organisations.Find(model.research.orgId);
            return View(model);
        }

        [Authorize(Roles = "admin")]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MailTemplate(FormCollection form)
        {
            var research = db.OrganisationsResearches.Find(Int32.Parse(form["researchId"]));
            research.mailTitle = form["mailTitle"];
            research.mailBody = form["mailBody"];
            db.Entry(research).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect($"/organisation/details/{research.orgId.ToString()}");
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
