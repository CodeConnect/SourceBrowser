using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SourceBrowser.Site.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Repository(string username, string repo, string query)
        {
            var x = new JsonResult();
            x.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return x;
        }
    }
}