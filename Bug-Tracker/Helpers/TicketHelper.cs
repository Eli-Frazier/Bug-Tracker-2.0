using Bug_Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Helpers
{
    public class TicketHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ProjectHelper projHelper = new ProjectHelper();

        public ICollection<Ticket> ListAssignedToUserTickets(string userId)
        {
            return db.Tickets.Where(t => t.AssignedToUserId == userId).ToList();
        }

        public ICollection<Ticket> ListOwnerUserTickets(string userId)
        {
            return db.Tickets.Where(t => t.OwnerUserId == userId).ToList();
        }

        public ICollection<Ticket> ListPMTickets(string userId)
        {
            //Create an empty list of Tickets that I can add to
            var pmTickets = new List<Ticket>();

            //Get all of the Projects for the PM using the ProjectsHelper
            var myProjects = projHelper.ListUserProjects(userId);
            foreach(var project in myProjects)
            {
                pmTickets.AddRange(db.Tickets.Where(t => t.ProjectId == project.Id));
            }

            //Show the data
            return pmTickets;
        }
    }
}