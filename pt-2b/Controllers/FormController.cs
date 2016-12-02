using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Text;

using pt_2b.Models;

namespace pt_2b.Controllers
{
    public class FormController : Controller
    {
        DataBase db = new DataBase();
        
        public ActionResult Index()
        {
            //костыль!
            //убирание полоски меню, чтобы не пугать анонимных пользователей
            ViewData["hideMenu"] = "hidden";

            string code = Request.Form["code"];

            if (Request.Form["action"] == "next")
            {
                if (db.Forms.Where(t => t.code == code).Count() > 0)
                {
                    Session.Remove("CheckMessage");
                    return Redirect("/form/filling?code=" + code);
                }
                //КОСТЫЛЬ!!!
                //вход для 360
                else if (db.THSUsers.Where(t => t.code == code && t.answered == 0).Count() > 0)
                {
                    return Redirect("/thsforms/filling?code=" + code);
                }
                //если 360 по этому коду уже заполнена, то перепройти её нельзя
                else if (db.THSUsers.Where(t => t.code == code && t.answered == 1).Count() > 0)
                {
                    Session.Remove("CheckMessage");
                    Session.Add("CheckMessage", String.Format("Вы уже заполнили эту анкету.", code));
                    return View();
                }
                //не найдено ни анкеты, ни 360 для указанного кода
                else
                {
                    Session.Remove("CheckMessage");
                    Session.Add("CheckMessage", String.Format("Анкеты с кодовым словом «{0}» не найдено.", code));
                    return View();
                }
            }
            else return View();
        }

        public ActionResult Filling()
        {
            //попробуем загрузить тест через сессию
            FormBox tBox = (FormBox)Session["tBox"];
            //если неудалось, то посмотрим код в параметрах запроса
            if (tBox == null && this.Request.QueryString["code"] != null)
            {
                tBox = new FormBox();
                string code = this.Request.QueryString["code"];
                tBox.form = (Form)db.Forms.Where(t => t.code == code).First();
                tBox.form = tBox.form.DeserializeFromXmlString(tBox.form.raw);

                //заменим \n на <br/>
                tBox.form.questions[tBox.currentQuestion].text = tBox.form.questions[tBox.currentQuestion].text.Replace("\\n", "<br/>");

                Session.Add("tBox", tBox);
                return View(tBox);
            }

            //если коробки с тестом до сих пор нет, нужно увести пользователя на страницу об ошибке
            if (tBox == null) return Redirect("/");

            //нажали кнопочку "Вперед"
            if (Request.Form["action"] == "next")
            {
                //жесткая альтернатива
                if (tBox.form.questions[tBox.currentQuestion].type == "hard")
                {
                    //ответ выбран
                    if(Request.Form["answerHard"] != null)
                    {
                        tBox.form.questions[tBox.currentQuestion].answer = Request.Form["answerHard"];

                        //сбрасываем ключи к секретным вопросам (сделано на случай использования кнопки назад и переответов на вопросы
                        tBox.form.questions[tBox.currentQuestion].keys = "";
                        //перебираем ответы вопроса
                        foreach (Answer a in tBox.form.questions[tBox.currentQuestion].answers)
                        {
                            //если у ответа есть ключ к секеретному вопросу
                            if (a.keyto > 0)
                            {
                                //и выбран именно этот ответ
                                if (a.Value == tBox.form.questions[tBox.currentQuestion].answer)
                                {
                                    //добавим ключ разблокировки на соответствующий вопрос
                                    tBox.form.questions[tBox.currentQuestion].keys += a.keyto.ToString() + ";";
                                }
                            }
                            
                            //проверяем, выбран ли этот вариант ответа
                            if (a.Value == tBox.form.questions[tBox.currentQuestion].answer)
                            {
                                //добавляем к ответу номер его позиции
                                tBox.form.questions[tBox.currentQuestion].answer += "@#@" + a.position.ToString();
                                //так как это жесткая альтернатива, дальше перебирать ответы нет смысла, выходим.
                                break;
                            }
                        }

                        tBox.currentQuestion++;
                    }
                    else if (tBox.form.questions[tBox.currentQuestion].answers.Count() == 0)
                    {
                        tBox.currentQuestion++;
                    }
                }
                //открытый вопрос
                else if (tBox.form.questions[tBox.currentQuestion].type == "text")
                {
                    tBox.form.questions[tBox.currentQuestion].answer = Request.Form["answerText"];
                    tBox.currentQuestion++;
                }
                //мягкая альтернатива
                else if (tBox.form.questions[tBox.currentQuestion].type == "soft")
                {
                    bool rangeExceed = false;
                    string answerString = "";

                    System.Collections.Hashtable answers = new System.Collections.Hashtable();
                    
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key.StartsWith("answer"))
                        {
                            string hKey = key.Replace("answer", "");
                            string hVal = Request.Form[key];
                            answers.Add(hKey, hVal);
                            answerString += hVal + "@#@" + hKey + "@%@";
                        }
                    }
                    if(answers.Count > 0) answerString = answerString.Substring(0, answerString.Length - 3);

                    if((tBox.form.questions[tBox.currentQuestion].minimum != 0 && answers.Count < tBox.form.questions[tBox.currentQuestion].minimum)
                        || (tBox.form.questions[tBox.currentQuestion].maximum != 0 && answers.Count > tBox.form.questions[tBox.currentQuestion].maximum))
                        rangeExceed = true;

                    if (!rangeExceed)
                    {
                        //сбрасываем ключи к секретным вопросам (сделано на случай использования кнопки назад и переответов на вопросы)
                        tBox.form.questions[tBox.currentQuestion].keys = "";
                        //перебираем ответы вопроса
                        foreach (Answer a in tBox.form.questions[tBox.currentQuestion].answers)
                        {
                            //если у ответа есть ключ к секеретному вопросу
                            if (a.keyto > 0)
                            {
                                //и этот ответ выбран
                                if (answers.ContainsKey(a.position.ToString()) && a.Value == answers[a.position.ToString()].ToString())
                                {
                                    //добавим ключ разблокировки на соответствующий вопрос
                                    tBox.form.questions[tBox.currentQuestion].keys += a.keyto.ToString() + ";";
                                }
                            }
                        }
                        
                        tBox.form.questions[tBox.currentQuestion].answer = answerString;

                        tBox.currentQuestion++;
                    }
                }

                while (true)
                {
                    //если следующий вопрос не последний
                    if (tBox.form.questions.Count() >= tBox.currentQuestion + 1
                        //и секретный
                        && tBox.form.questions[tBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        //перебираем все вопросы
                        foreach (Question q in tBox.form.questions)
                        {
                            //если в ответах есть ключ к этому вопросу, то
                            if (q.keys.Contains((tBox.form.questions[tBox.currentQuestion].position).ToString()))
                            {
                                //разблокируем его
                                haveKey = true;
                                //и выходим из цикла
                                break;
                            }
                        }
                        //если ключ так и не найден, то берём следующий вопрос
                        if (!haveKey) tBox.currentQuestion++;
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
                if(tBox.currentQuestion > 0) tBox.currentQuestion--;

                //повторяем операцию проверки секретного вопроса и наличия ответа на него как в примере с конопкой "Вперед"
                while (true)
                {
                    //проверим, является ли предыдущий вопрос секретным и есть ли на него ключ
                    if (1 <= tBox.currentQuestion + 1
                        && tBox.form.questions[tBox.currentQuestion].isSecret == "true")
                    {
                        bool haveKey = false;
                        foreach (Question q in tBox.form.questions)
                        {
                            if (q.keys.Contains((tBox.form.questions[tBox.currentQuestion].position).ToString()))
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
            if (tBox.form.questions.Count() < tBox.currentQuestion + 1)
            {
                //подготавливаем и сохраняем результат в БД
                string s = tBox.form.SerializeToXmlString();
                FormAnswer ta = new FormAnswer();
                ta.testUsername = tBox.form.username;
                ta.testCode = tBox.form.code;
                ta.raw = s;
                ta.dateCreate = DateTime.Now;
                db.FormAnswers.Add(ta);
                db.SaveChanges();
                //удаляем коробку с тестом из сессии
                Session.Remove("tBox");
                //показываем заключительный экран
                return View("Finish");
            }
            else
            {
                //заменим \n на <br/>
                tBox.form.questions[tBox.currentQuestion].text = tBox.form.questions[tBox.currentQuestion].text.Replace("\\n", "<br/>");
            }

            //сохраним результат в сессию
            Session.Remove("tBox");
            Session.Add("tBox", tBox);

            return View(tBox);
        }

        //метод выгрузки результатов в виде csv
        [Authorize(Roles = "admin")]
        public ActionResult Results()
        {
            string code = HttpContext.Request.QueryString.Get("code");
            Form form = (Form)db.Forms.Where(x => x.code == code).Single();
            form = form.DeserializeFromXmlString(form.raw);
            int answersCount = 0;
            string csv = "";

            //подсчитываем количество ответов и формируем нулевую строку файла
            foreach (Question q in form.questions)
            {
                answersCount++;

                //вставляем q0 если есть преамбула
                if (answersCount == 1 && q.answers.Count() == 0)
                {
                    answersCount--;
                }

                csv += ";q" + answersCount.ToString();
            }
            csv += ";\r\n";
            answersCount = 0;

            //здесь начинается костыль со сменой кодировки результатов из UTF-8 в WIN1251. временное решение
            Encoding srcEnc = Encoding.UTF8;
            Encoding destEnc = Encoding.GetEncoding(1251);

            //перебираем ответы
            foreach (FormAnswer ta in db.FormAnswers.Where(x => x.testCode == code))
            {
                //идентификатор ответа из БД
                csv += ta.id.ToString() + ";";

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
                    //если в ответе содержится специальный разделитель, значит это мягкая альтернатива и в файл результатов нужно добавить номера выбранных ответов
                    if (ans.Contains("@%@"))
                    {
                        foreach (string answer in Regex.Split(ans, "@%@"))
                        {
                            csv += Regex.Split(answer, "@#@")[1] + ",";
                        }
                        csv = csv.TrimEnd(',');
                    }
                    //если в ответе содержится специальный разделитель, значит это жесткая альтернатива и номер ответа идёт после его текста
                    else if (ans.Contains("@#@"))
                    {
                        csv += Regex.Split(ans, "@#@")[1];
                    }
                    else csv += ans;
                    csv +=  ";";
                }

                csv += "\r\n";
                answersCount++;
            }
            
            string filename = String.Format("pt-{0}-{1}.csv", form.code, answersCount.ToString()); ;
            return File(destEnc.GetBytes(csv), "text/csv", filename);
        }
    }
}
