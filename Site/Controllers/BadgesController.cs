using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace CardWall.Controllers
{
    public class BadgesController : Controller
    {
        public ActionResult Index()
        {
            var badges = (BadgeConfiguration)ConfigurationManager.GetSection("Badges");
            return View(badges);
        }

    }
}
