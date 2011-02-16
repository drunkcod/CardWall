using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

namespace CardWall
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var quickLinks = (QuickLinkConfiguration)ConfigurationManager.GetSection("QuickLinks");
            foreach(var link in quickLinks.Links) 
                routes.MapRoute(link.Path, link.Path, new { controller = "Projects", action = "CurrentIteration", projects = link.Projects });

            routes.MapRoute("CurrentIteration", "CurrentIteration", new { controller = "Projects", action = "CurrentIteration" }); 
            routes.MapRoute("Default", "{controller}/{id}/{action}", new { controller = "Projects", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);
        }
    }
}