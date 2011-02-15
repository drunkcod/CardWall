using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CardWall
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("CurrentIteration", "CurrentIteration", new { controller = "Projects", action = "CurrentIteration" }); 
            routes.MapRoute("Default", "{controller}/{id}/{action}", new { controller = "Projects", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);
        }
    }
}