using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bug_Tracker.Models;

namespace Bug_Tracker.Helpers
{
    public class NotificationHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ICollection<TicketNotification> ListUserNotifications(string userId)
        {
            return db.TicketNotifications.Where(t => t.UserId == userId).ToList();
        }
    }
}