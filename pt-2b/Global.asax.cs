using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace pt_2b
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //МЕТОД СОДЕРЖИТ КОСТЫЛЬ ДЛЯ СОХРАНЕНИЯ ПРОГРЕССА ЗАПОЛНЕНИЯ АНКЕТЫ ПОСЛЕ ЗАКРЫТИЯ БРАУЗЕРА
        protected void Application_PostMapRequestHandler(object sender, EventArgs e)
        {
            //КОСТЫЛЬ!
            try
            {
                if (Request.Cookies["ASP.NET_SessionIdTemp"] != null)
                {
                    if (Request.Cookies["ASP.NET_SessionId"] == null)
                        Request.Cookies.Add(new HttpCookie("ASP.NET_SessionId", Request.Cookies["ASP.NET_SessionIdTemp"].Value));
                    else
                        Request.Cookies["ASP.NET_SessionId"].Value = Request.Cookies["ASP.NET_SessionIdTemp"].Value;
                }
            }
            catch (Exception) { }
            //-------------------------------------------------------------------
        }

        //МЕТОД СОДЕРЖИТ КОСТЫЛЬ ДЛЯ СОХРАНЕНИЯ ПРОГРЕССА ЗАПОЛНЕНИЯ АНКЕТЫ ПОСЛЕ ЗАКРЫТИЯ БРАУЗЕРА
        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            //КОСТЫЛЬ!
            try
            {
                HttpCookie cookie = new HttpCookie("ASP.NET_SessionIdTemp", Session.SessionID);
                cookie.Expires = DateTime.Now.AddMinutes(Session.Timeout);
                Response.Cookies.Add(cookie);
            }
            catch (Exception) { }
            //-------------------------------------------------------------------
        }


    }
}
