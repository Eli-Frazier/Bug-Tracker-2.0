using Bug_Tracker.Models;
using Bug_Tracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker.Controllers
{
    
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Index()
        {
            var VM = new IndexVM
            {
                Tickets = db.Tickets.ToList(),
                Projects = db.Projects.ToList(),
                TicketHistories = db.TicketHistories.ToList()
            };
            return View(VM);
        }

        public ActionResult LandingPage()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ProjectDetail(int id)
        {
            return View("Index", db.Projects.Find(id));
        }

            }
}