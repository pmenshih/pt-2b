using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

using pt_2b.Models;

namespace pt_2b.Controllers
{
    public class TestController : Controller
    {
        DataBase db = new DataBase();

        // GET: Test
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult CodeCheck(string code)
        {
            //костыль!
            ViewData["hideMenu"] = "hidden";

            if (db.Tests.Where(t => t.code == code).Count() > 0)
            {
                return JavaScript("window.location = '/test/filling?code=" + code + "'");
            }
            else
                return PartialView("_ViewErrorMessage", new Models.ViewMessage { message = String.Format("Анкеты с кодовым словом «{0}» не найдено.", code) });
        }

        public ActionResult Filling()
        {
            //костыль!
            ViewData["hideMenu"] = "hidden";

            //загрузим тест
            TestBox tBox = (TestBox)Session["tBox"];
            if (tBox == null && this.Request.QueryString["code"] != null)
            {
                tBox = new TestBox();
                string code = this.Request.QueryString["code"];
                tBox.test = (Test)db.Tests.Where(t => t.code == code).First();
                tBox.test = tBox.test.DeserializeFromXmlString(tBox.test.raw);
                //заменим \n на <br/>
                tBox.test.questions[tBox.currentQuestion].text = tBox.test.questions[tBox.currentQuestion].text.Replace("\\n", "<br/>");
                Session.Add("tBox", tBox);
                return View(tBox);
            }
            
            //если коробки до сих пор нет, нужно увести пользователя на страницу об ошибке
            if (tBox == null) return Redirect("/");

            //нажали кнопочку "Вперед"
            if (Request.Form["action"] == "next")
            {
                //жесткая альтернатива
                if (tBox.test.questions[tBox.currentQuestion].type == "hard")
                {
                    //ответ выбран
                    if(Request.Form["answerHard"] != null)
                    {
                        tBox.test.questions[tBox.currentQuestion].answer = Request.Form["answerHard"];

                        //если у ответа есть ключ к секретному вопросу
                        tBox.test.questions[tBox.currentQuestion].keys = "";
                        foreach (Answer a in tBox.test.questions[tBox.currentQuestion].answers)
                        {
                            if (a.keyto > 0)
                            {
                                //поставим флаг разблокировки на соответствующий вопрос
                                if (a.Value == tBox.test.questions[tBox.currentQuestion].answer)
                                {
                                    tBox.test.questions[tBox.currentQuestion].keys += a.keyto.ToString() + ";";
                                }
                            }

                            if (a.Value == tBox.test.questions[tBox.currentQuestion].answer)
                            {
                                tBox.test.questions[tBox.currentQuestion].answer += "@#@" + a.position.ToString();
                            }
                        }

                        tBox.currentQuestion++;
                    }
                }
                //открытый вопрос
                else if (tBox.test.questions[tBox.currentQuestion].type == "text")
                {
                    tBox.test.questions[tBox.currentQuestion].answer = Request.Form["answerText"];
                    tBox.currentQuestion++;
                }

                while (true)
                {
                    //проверим, является ли следующий вопрос секретным и есть ли на него ключ
                    if (tBox.test.questions.Count() >= tBox.currentQuestion + 1
                        && tBox.test.questions[tBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        foreach (Question q in tBox.test.questions)
                        {
                            if (q.keys.Contains((tBox.currentQuestion + 1).ToString()))
                            {
                                haveKey = true;
                                break;
                            }
                        }
                        if (!haveKey) tBox.currentQuestion++;
                        else break;
                    }
                    else break;
                }
            }
            //нажали кнопочку "Назад"
            else if (Request.Form["action"] == "prev")
            {
                if(tBox.currentQuestion > 0) tBox.currentQuestion--;

                while (true)
                {
                    //проверим, является ли предыдущий вопрос секретным и есть ли на него ключ
                    if (1 <= tBox.currentQuestion + 1
                        && tBox.test.questions[tBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        foreach (Question q in tBox.test.questions)
                        {
                            if (q.keys.Contains((tBox.currentQuestion + 1).ToString()))
                            {
                                haveKey = true;
                                break;
                            }
                        }
                        if(!haveKey) tBox.currentQuestion--;
                        else break;
                    }
                    else break;
                }
            }

            //проверим доступность кнопки "Назад"
            if (tBox.currentQuestion > 0) tBox.disableBack = "";
            else tBox.disableBack = "disabled";

            //тест закончился
            if(tBox.test.questions.Count() < tBox.currentQuestion + 1)
            {
                string s = tBox.test.SerializeToXmlString();
                TestAnswer ta = new TestAnswer();
                ta.testUsername = tBox.test.username;
                ta.testCode = tBox.test.code;
                ta.raw = s;
                ta.dateCreate = DateTime.Now;
                db.TestAnswers.Add(ta);
                db.SaveChanges();

                Session.Remove("tBox");

                return View("Finish");
            }

            //сохраним результат в сессию
            Session.Remove("tBox");
            Session.Add("tBox", tBox);

            return View(tBox);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Results()
        {
            string code = HttpContext.Request.QueryString.Get("code");
            Test test = (Test)db.Tests.Where(x => x.code == code).Single();
            test = test.DeserializeFromXmlString(test.raw);
            int answersCount = 0;
            string csv = "";

            foreach (Question q in test.questions)
            {
                answersCount++;
                csv += ";q" + answersCount.ToString();
            }
            csv += ";\r\n";
            answersCount = 0;

            foreach (TestAnswer ta in db.TestAnswers.Where(x => x.testCode == code))
            {
                csv += ta.id.ToString() + ";";

                Test tBoxa = test.DeserializeFromXmlString(ta.raw);
                foreach (Question q in tBoxa.questions)
                {
                    if (q.answer.Contains("@#@"))
                    {
                        csv += Regex.Split(q.answer, "@#@")[1];
                    }
                    else csv += q.answer;
                    csv +=  ";";
                }

                csv += "\r\n";
                answersCount++;
            }
            
            string filename = String.Format("pt-{0}-{1}.csv", test.code, answersCount.ToString()); ;
            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", filename);
        }
    }
}