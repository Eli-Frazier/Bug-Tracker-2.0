using Bug_Tracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Bug_Tracker.Extensions
{
    public static class TicketExtensions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static void RecordChanges(this Ticket ticket, Ticket oldTicket)
        {
            var propertyList = WebConfigurationManager.AppSettings["TrackedTicketProperties"].Split(',');

            foreach(PropertyInfo prop in ticket.GetType().GetProperties())
            {
                if (!propertyList.Contains(prop.Name))
                    continue;

                var value = prop.GetValue(ticket, null) ?? "";
                var oldValue = prop.GetValue(oldTicket, null) ?? "";

                if(value.ToString() != oldValue.ToString())
                {
                    var ticketHistory = new TicketHistory
                    {
                        Changed = DateTimeOffset.Now,
                        Property = prop.Name,
                        NewValue = GetValueFromKey(prop.Name, value),
                        OldValue = GetValueFromKey(prop.Name, oldValue),
                        TicketId = ticket.Id,
                        UserId = HttpContext.Current.User.Identity.GetUserId()
                    };

                    db.TicketHistories.Add(ticketHistory);
                }
            }
            db.SaveChanges();
        }

        private static string GetValueFromKey(string keyName, object key)
        {
            var returnValue = "";
            if (key.ToString() == string.Empty)
                return returnValue;

            switch (keyName)
            {
                case "ProjectId":
                    returnValue = db.Projects.Find(key).Name;
                    break;
                case "TicketTypeId":
                    returnValue = db.TicketTypes.Find(key).Name;
                    break;
                case "TicketPriorityId":
                    returnValue = db.TicketPriorities.Find(key).Name;
                    break;
                case "TicketStatusId":
                    returnValue = db.TicketStatus.Find(key).Name;
                    break;
                case "OwnerUserId":
                case "AssignedToUserId":
                    returnValue = db.Users.Find(key).DisplayName;
                    break;
                default:
                    returnValue = key.ToString();
                    break;
            }
            return returnValue;
        }

        public static async Task TriggerNotifications(this Ticket ticket, Ticket oldTicket)
        {
            //Let's begin with notifications for Ticket Assignment/UnAssignment
            var newAssignment = (ticket.AssignedToUserId != null && oldTicket.AssignedToUserId == null);
            var unAssignment = (ticket.AssignedToUserId == null && oldTicket.AssignedToUserId != null);
            var reAssignment = ((ticket.AssignedToUserId != null && oldTicket.AssignedToUserId != null) &&
                               (ticket.AssignedToUserId != oldTicket.AssignedToUserId));

            //Compose the body of the email
            var body = new StringBuilder();
            body.AppendFormat("<p>Email From: <bold>{0}</bold>({1})</p>", "Administrator", WebConfigurationManager.AppSettings["emailfrom"]);
            body.AppendLine("<br/><p><u><b>Message:</b></u></p>");
            body.AppendFormat("<p><b>Project Name:</b> {0}</p>", db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId).Name);
            body.AppendFormat("<p><b>Ticket Title:</b> {0} | Id: {1}</p>", ticket.Title, ticket.Id);
            body.AppendFormat("<p><b>Ticket Created:</b> {0}</p>", ticket.Created);
            body.AppendFormat("<p><b>Ticket Type:</b> {0}</p>", db.TicketTypes.Find(ticket.TicketTypeId).Name);
            body.AppendFormat("<p><b>Ticket Status:</b> {0}</p>", db.TicketStatus.Find(ticket.TicketStatusId).Name);
            body.AppendFormat("<p><b>Ticket Priority:</b> {0}</p>", db.TicketPriorities.Find(ticket.TicketPriorityId).Name);

            //Generate email
            IdentityMessage email = null;
            if (newAssignment)
            {
                //Generate 1 email to the new Developer letting them know they have been assigned
                email = new IdentityMessage()
                {
                    Subject = "Notification: A Ticket has been assigned to you...",
                    Body = body.ToString(),
                    Destination = db.Users.Find(ticket.AssignedToUserId).Email
                };

                var svc = new EmailService();
                await svc.SendAsync(email);
            }
            else if (unAssignment)
            {
                //Generate 1 email to the old Developer letting them know they have been unassigned
                email = new IdentityMessage()
                {
                    Subject = "Notification: You have been taken off of a Ticket...",
                    Body = body.ToString(),
                    Destination = db.Users.Find(oldTicket.AssignedToUserId).Email
                };

                var svc = new EmailService();
                await svc.SendAsync(email);
            }
            else if (reAssignment)
            {
                //Generate 1 email to the new Developer letting them know they have been assigned
                email = new IdentityMessage()
                {
                    Subject = "Notification: A Ticket has been assigned to you...",
                    Body = body.ToString(),
                    Destination = db.Users.Find(ticket.AssignedToUserId).Email
                };

                var svc = new EmailService();
                await svc.SendAsync(email);

                //Generate 1 email to the old Developer letting them know they have been unassigned
                email = new IdentityMessage()
                {
                    Subject = "Notification: You have been taken off of a Ticket...",
                    Body = body.ToString(),
                    Destination = db.Users.Find(oldTicket.AssignedToUserId).Email
                };

                svc = new EmailService();
                await svc.SendAsync(email);
            }

            //Generate Notification
            TicketNotification notification = null;
            if (newAssignment)
            {
                notification = new TicketNotification
                {
                    Body = "Notification: A Ticket has been assigned to you...<br />" + body.ToString(),
                    UserId = ticket.AssignedToUserId,
                    TicketId = ticket.Id
                };
                db.TicketNotifications.Add(notification);
            }
            else if (unAssignment)
            {
                notification = new TicketNotification
                {
                    Body = "Notification: You have been taken off of a Ticket...<br />" + body.ToString(),
                    UserId = oldTicket.AssignedToUserId,
                    TicketId = ticket.Id
                };
                db.TicketNotifications.Add(notification);
            }
            else if (reAssignment)
            {
                notification = new TicketNotification
                {
                    Body = "Notification: A Ticket has been assigned to you...<br />" + body.ToString(),
                    UserId = ticket.AssignedToUserId,
                    TicketId = ticket.Id
                };
                db.TicketNotifications.Add(notification);

                notification = new TicketNotification
                {
                    Body = "Notification: You have been taken off of a Ticket...<br />" + body.ToString(),
                    UserId = oldTicket.AssignedToUserId,
                    TicketId = ticket.Id
                };
                db.TicketNotifications.Add(notification);
            }

            db.SaveChanges();
        }
    }
}