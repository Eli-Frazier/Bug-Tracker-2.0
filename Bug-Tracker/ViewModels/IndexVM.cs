

using Bug_Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.ViewModels
{
    public class IndexVM
    {
        public List<Ticket> Tickets { get; set; }
        public List<Project> Projects { get; set; }
        public List<TicketHistory> TicketHistories { get; set; }

        public IndexVM()
        {
            Tickets = new List<Ticket>();
            Projects = new List<Project>();
            TicketHistories = new List<TicketHistory>();
        }
        
    }
}