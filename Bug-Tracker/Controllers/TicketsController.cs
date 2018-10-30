using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Bug_Tracker.Extensions;
using Bug_Tracker.Helpers;
using Bug_Tracker.Models;
using Bug_Tracker.ViewModels;
using Microsoft.AspNet.Identity;

namespace Bug_Tracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private TicketHelper ticketHelper = new TicketHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET: Tickets
        public ActionResult Index()
        {
            //Tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList(),
            var myIndexVM = new IndexVM
            {
                Tickets = db.Tickets.ToList(),
                Projects = db.Projects.ToList()
            };
        
            return View(myIndexVM);
        }

        public ActionResult AssignedToTickets()
        {
            var myIndexVM = new IndexVM
            {
                Tickets = ticketHelper.ListAssignedToUserTickets(User.Identity.GetUserId()).ToList(),
                Projects = projectHelper.ListUserProjects(User.Identity.GetUserId()).ToList()
            };
            return View("Index", myIndexVM);
        }

        public ActionResult OwnedTickets()
        {
            var myIndexVM = new IndexVM
            {
                Tickets = ticketHelper.ListOwnerUserTickets(User.Identity.GetUserId()).ToList(),
                Projects = projectHelper.ListUserProjects(User.Identity.GetUserId()).ToList()
            };
            return View("Index", myIndexVM);
        }

        public ActionResult PMTickets()
        {
            var myIndexVM = new IndexVM
            {
                Tickets = ticketHelper.ListPMTickets(User.Identity.GetUserId()).ToList(),
                Projects = projectHelper.ListUserProjects(User.Identity.GetUserId()).ToList()
            };
            return View("Index", myIndexVM);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(int id)
        {
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            ViewBag.ProjectId = id;
            
            var userId = User.Identity.GetUserId();
            var myRole = roleHelper.ListUserRoles(userId).ToList().FirstOrDefault();
            switch (myRole)
            {
                case "Submitter":
                    if (!projectHelper.IsUserOnProject(userId, id))
                        return RedirectToAction("OwnedTickets", "Tickets");
                    break;
                default:
                    break;
            }

            return View();
        }

        //POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectId,Title,Description,TicketPriorityId,TicketStatusId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = User.Identity.GetUserId();

                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? Id)
        {

            var ticket = db.Tickets.Find(Id);
            var userId = User.Identity.GetUserId();
            var myRole = roleHelper.ListUserRoles(userId).ToList().FirstOrDefault();
            switch (myRole)
            {
                case "Developer":
                    if (ticket.AssignedToUserId != userId)
                        return RedirectToAction("Details", "Tickets", new { Id });
                    break;
                case "Submitter":
                    if (ticket.OwnerUserId != userId)
                        return RedirectToAction("Details", "Tickets", new { Id });
                    break;
                case "Project Manager":
                    if (!projectHelper.IsUserOnProject(userId, ticket.ProjectId))
                        return RedirectToAction("Details", "Tickets", new { Id });
                    break;
                default:
                    break;
            }

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var projDevs = new List<ApplicationUser>();
            var projUsers = projectHelper.UsersOnProject(ticket.ProjectId);
            foreach(var user in projUsers)
            {
                if (roleHelper.IsUserInRole(user.Id, "Developer"))
                {
                    projDevs.Add(user);
                }
            }

            ViewBag.AssignedToUserId = new SelectList(projDevs, "Id", "Email", ticket.AssignedToUserId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ProjectId,Title,Description,Created,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToUserId,OwnerUserId")] Ticket ticket)
        {
            var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.Now;
                db.Tickets.Add(ticket);
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                if(string.IsNullOrEmpty(oldTicket.AssignedToUserId) && !string.IsNullOrEmpty(ticket.AssignedToUserId))
                {
                    ticket.TicketStatusId = db.TicketStatus.FirstOrDefault(t => t.Name == "Assigned").Id;
                }

                ticket.RecordChanges(oldTicket);
                await ticket.TriggerNotifications(oldTicket);

                return RedirectToAction("Details", "Tickets", new { id = ticket.Id});
            }

            

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
