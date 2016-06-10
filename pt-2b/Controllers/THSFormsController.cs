using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using pt_2b.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace pt_2b.Controllers
{
    public class THSFormsController : Controller
    {
        private DataBase db = new DataBase();

        // GET: THSForms
        public ActionResult Index()
        {
            return View(db.THSForms.ToList());
        }

        // GET: THSForms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSForm tHSForm = db.THSForms.Find(id);
            if (tHSForm == null)
            {
                return HttpNotFound();
            }
            return View(tHSForm);
        }

        // GET: THSForms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: THSForms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,organisationId")] THSForm tHSForm)
        {
            if (ModelState.IsValid)
            {
                db.THSForms.Add(tHSForm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tHSForm);
        }

        // GET: THSForms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSForm tHSForm = db.THSForms.Find(id);
            if (tHSForm == null)
            {
                return HttpNotFound();
            }
            return View(tHSForm);
        }

        // POST: THSForms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,organisationId")] THSForm tHSForm)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tHSForm).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tHSForm);
        }

        // GET: THSForms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            THSForm tHSForm = db.THSForms.Find(id);
            if (tHSForm == null)
            {
                return HttpNotFound();
            }
            return View(tHSForm);
        }

        // POST: THSForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            THSForm tHSForm = db.THSForms.Find(id);
            db.THSForms.Remove(tHSForm);
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

        public ActionResult Start()
        {
            return View();
        }

        public ActionResult CodeCheck(string code)
        {
            if (db.THSUsers.Where(t => t.code == code && t.answered == 0).Count() > 0)
            {
                return JavaScript("window.location = '/thsforms/filling?code=" + code + "'");
            }
            else if (db.THSUsers.Where(t => t.code == code && t.answered == 1).Count() > 0)
            {
                return PartialView("_ViewErrorMessage", new Models.ViewMessage { message = String.Format("Вы уже заполнили эту анкету.", code) });
            }
            else
                return PartialView("_ViewErrorMessage", new Models.ViewMessage { message = String.Format("Анкеты с кодом «{0}» не найдено.", code) });
        }

        public ActionResult Filling()
        {
            //попробуем загрузить тест через сессию
            FormBox thsBox = (FormBox)Session["thsBox"];
            //если неудалось, то посмотрим код в параметрах запроса
            if (thsBox == null && this.Request.QueryString["code"] != null)
            {
                thsBox = new FormBox();
                thsBox.form = new Form();
                string code = this.Request.QueryString["code"];
                THSUser tu = (THSUser)db.THSUsers.Where(t => t.code == code).First();
                thsBox.form = thsBox.form.DeserializeFromXmlString(tu.raw);

                //ИЗМЕНЕНИЕ ОТНОСИТЕЛЬНО АНКЕТЫ!!!
                thsBox.form.code = code;
                thsBox.form.username = tu.id.ToString();

                //заменим \n на <br/>
                thsBox.form.questions[thsBox.currentQuestion].text = thsBox.form.questions[thsBox.currentQuestion].text.Replace("\\n", "<br/>");

                //КОСТЫЛЬ!!!
                //заменим @utype@ на тип связи и аболдим её
                string uType = "";
                switch (tu.thsUType)
                {
                    case 1:
                        uType = "<b>самого себя</b>, ";
                        break;
                    case 2:
                        uType = "вашего <b>коллеги</b>,";
                        break;
                    case 3:
                        uType = "вашего <b>начальника</b>,";
                        break;
                    case 4:
                        uType = "вашего <b>подчинённого</b>,";
                        break;
                    case 5:
                        uType = "вашего <b></b>,";
                        break;
                    case 6:
                        uType = "вашего <b>партнёра</b>,";
                        break;
                }
                thsBox.form.questions[thsBox.currentQuestion].text = thsBox.form.questions[thsBox.currentQuestion].text.Replace("@utype@", uType);

                Session.Add("thsBox", thsBox);
                return View(thsBox);
            }

            //если коробки с тестом до сих пор нет, нужно увести пользователя на страницу об ошибке
            if (thsBox == null) return Redirect("/");

            //нажали кнопочку "Вперед"
            if (Request.Form["action"] == "next")
            {
                //жесткая альтернатива
                if (thsBox.form.questions[thsBox.currentQuestion].type == "hard")
                {
                    //ответ выбран
                    if (Request.Form["answerHard"] != null)
                    {
                        thsBox.form.questions[thsBox.currentQuestion].answer = Request.Form["answerHard"];

                        //сбрасываем ключи к секретным вопросам (сделано на случай использования кнопки назад и переответов на вопросы
                        thsBox.form.questions[thsBox.currentQuestion].keys = "";
                        //перебираем ответы вопроса
                        foreach (Answer a in thsBox.form.questions[thsBox.currentQuestion].answers)
                        {
                            //если у ответа есть ключ к секеретному вопросу
                            if (a.keyto > 0)
                            {
                                //и выбран именно этот ответ
                                if (a.Value == thsBox.form.questions[thsBox.currentQuestion].answer)
                                {
                                    //добавим ключ разблокировки на соответствующий вопрос
                                    thsBox.form.questions[thsBox.currentQuestion].keys += a.keyto.ToString() + ";";
                                }
                            }

                            //проверяем, выбран ли этот вариант ответа
                            if (a.Value == thsBox.form.questions[thsBox.currentQuestion].answer)
                            {
                                //добавляем к ответу номер его позиции
                                thsBox.form.questions[thsBox.currentQuestion].answer += "@#@" + a.position.ToString();
                                //так как это жесткая альтернатива, дальше перебирать ответы нет смысла, выходим.
                                break;
                            }
                        }

                        thsBox.currentQuestion++;
                    }
                }
                //открытый вопрос
                else if (thsBox.form.questions[thsBox.currentQuestion].type == "text")
                {
                    thsBox.form.questions[thsBox.currentQuestion].answer = Request.Form["answerText"];
                    thsBox.currentQuestion++;
                }

                while (true)
                {
                    //если следующий вопрос не последний
                    if (thsBox.form.questions.Count() >= thsBox.currentQuestion + 1
                        //и секретный
                        && thsBox.form.questions[thsBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        //перебираем все вопросы
                        foreach (Question q in thsBox.form.questions)
                        {
                            //если в ответах есть ключ к этому вопросу, то
                            if (q.keys.Contains((thsBox.currentQuestion + 1).ToString()))
                            {
                                //разблокируем его
                                haveKey = true;
                                //и выходим из цикла
                                break;
                            }
                        }
                        //если ключ так и не найден, то берём следующий вопрос
                        if (!haveKey) thsBox.currentQuestion++;
                        //в противном случае выходим из бесконечного цикла
                        else break;
                    }
                    //если вопросы закончились, или следующий вопрос не секретный, то выходим из бесконечного цикла
                    else break;
                }
            }
            //нажали кнопочку "Назад"
            else if (Request.Form["action"] == "prev")
            {
                if (thsBox.currentQuestion > 0) thsBox.currentQuestion--;

                //повторяем операцию проверки секретного вопроса и наличия ответа на него как в примере с конопкой "Вперед"
                while (true)
                {
                    //проверим, является ли предыдущий вопрос секретным и есть ли на него ключ
                    if (1 <= thsBox.currentQuestion + 1
                        && thsBox.form.questions[thsBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        foreach (Question q in thsBox.form.questions)
                        {
                            if (q.keys.Contains((thsBox.currentQuestion + 1).ToString()))
                            {
                                haveKey = true;
                                break;
                            }
                        }
                        if (!haveKey) thsBox.currentQuestion--;
                        else break;
                    }
                    else break;
                }
            }

            //проверим доступность кнопки "Назад"
            if (thsBox.currentQuestion > 0) thsBox.disableBack = "";
            else thsBox.disableBack = "disabled";

            //тест закончился
            if (thsBox.form.questions.Count() < thsBox.currentQuestion + 1)
            {
                //подготавливаем и сохраняем результат в БД
                string s = thsBox.form.SerializeToXmlString();

                int thsuId = Int32.Parse(thsBox.form.username);
                THSUser thsu = db.THSUsers.Where(t => t.id == thsuId).First();
                thsu.raw = s;
                thsu.answered = 1;

                db.Entry(thsu).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //удаляем коробку с тестом из сессии
                Session.Remove("thsBox");
                //показываем заключительный экран
                return View("Finish");
            }

            //сохраним результат в сессию
            Session.Remove("thsBox");
            Session.Add("thsBox", thsBox);

            return View(thsBox);
        }

        //метод выгрузки результатов в виде csv
        [Authorize(Roles = "admin")]
        public ActionResult Results()
        {
            string code = HttpContext.Request.QueryString.Get("id");
            int thsuId = Int32.Parse(code);
            THSUser thsu = (THSUser)db.THSUsers.Where(x => x.thsId == thsuId && x.answered == 1).First();
            Form form = new Form();
            form = form.DeserializeFromXmlString(thsu.raw);
            int answersCount = 0;
            string csv = "";

            //подсчитываем количество ответов и формируем нулевую строку файла
            csv += ";t";
            foreach (Question q in form.questions)
            {
                answersCount++;
                csv += ";q" + answersCount.ToString();
            }
            csv += ";\r\n";
            answersCount = 0;

            //здесь начинается костыль со сменой кодировки результатов из UTF-8 в WIN1251. временное решение
            Encoding srcEnc = Encoding.UTF8;
            Encoding destEnc = Encoding.GetEncoding(1251);

            //перебираем ответы
            foreach (THSUser ta in db.THSUsers.Where(x => x.thsId == thsuId && x.answered == 1))
            {
                //идентификатор ответа из БД
                csv += ta.id.ToString() + ";";

                //тип оценщика
                csv += ta.thsUType.ToString() + ";";

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

            string filename = String.Format("360-{0}-{1}.csv", thsu.thsId.ToString(), answersCount.ToString()); ;
            return File(destEnc.GetBytes(csv), "text/csv", filename);
        }

        public ActionResult SendEmails(int id)
        {
            string query = "select tu.code as code, u.email as email, u.name as name, u.patronim as patronim, u.surname as surname, tu.thsUType as utype from thsusers tu, users u where tu.thsid = " + id.ToString() + " and tu.userid=u.id and tu.answered=0";
            var users = db.Database.SqlQuery<usrs>(query).ToList();
            foreach (var user in users)
            {
                //SmtpClient smtpClient = new SmtpClient(WebConfigurationManager.AppSettings["siteMailHost"], Int32.Parse(WebConfigurationManager.AppSettings["siteMailPort"]));
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential("psycho.mail.robot@gmail.com", "123456qW");

                MailMessage mail = new MailMessage();
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("office@psycho.ru", "Почтовый робот psycho.ru");
                mail.To.Add(new MailAddress(user.email));

                //КОСТЫЛЬ!!!
                //заменим @utype@ на тип связи и аболдим её
                string uType = "";
                switch (user.utype)
                {
                    case 1:
                        mail.Subject = "Оцените самого себя на test.psycho.ru";
                        uType = "<b>самого себя</b>";
                        break;
                    case 2:
                        mail.Subject = "Оцените коллегу на test.psycho.ru";
                        uType = "вашего <b>коллегу</b>";
                        break;
                    case 3:
                        mail.Subject = "Оцените начальника на test.psycho.ru";
                        uType = "вашего <b>начальника</b>";
                        break;
                    case 4:
                        mail.Subject = "Оцените подчинённого на test.psycho.ru";
                        uType = "вашего <b>подчинённого</b>";
                        break;
                    case 5:
                        mail.Subject = "Оцените  на test.psycho.ru";
                        uType = "вашего <b></b>";
                        break;
                    case 6:
                        mail.Subject = "Оцените партнёра на test.psycho.ru";
                        uType = "вашего <b>партнёра</b>";
                        break;
                }

                
                mail.Body = "Здравствуйте!<br/><br/>Вы получили это письмо, потому как ваш адрес указан в списке анкетируемых.<br/>Вам предлагается оценить " + uType + ".<br/><a href=\"http://test.psycho.ru/THSForms/Filling?code=" + user.code + "\">Для заполнения анкеты перейдите по ссылке</a>.<br/>Ваш код: <b>" + user.code + "</b><br/><br/>С уважением, команда psycho.ru.";
                

                smtpClient.Send(mail);
            }
            

            return Redirect("/THSForms/Details/" + id.ToString());
        }
    }

    public class usrs
    {
        public string code { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string patronim { get; set; }
        public string surname { get; set; }
        public int utype { get; set; }
    }
}
