namespace Bug_Tracker.Migrations
{
    using Bug_Tracker.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Bug_Tracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Bug_Tracker.Models.ApplicationDbContext context)
        {
            //create a few roles (admin and moderator)
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            //create users 
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "eli_frazier@yahoo.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "eli_frazier@yahoo.com",
                    Email = "eli_frazier@yahoo.com",
                    FirstName = "Eli",
                    LastName = "Frazier",
                    DisplayName = "Eli-Frazier",
                }, "LavabombAdmin1");
            }

            if (!context.Users.Any(u => u.Email == "demosub@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "demosub@mailinator.com",
                    Email = "demosub@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Submitter",
                    DisplayName = "Demo-Sub1",
                }, "DemoPassword321!");
            }
            if(!context.Users.Any(u => u.Email == "demodev@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "demodev@mailinator.com",
                    Email = "demodev@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer",
                    DisplayName = "Demo-Dev1",
                }, "DemoPassword321!");
            }
            if (!context.Users.Any(u => u.Email == "demopm@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "demopm@mailinator.com",
                    Email = "demopm@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Manager",
                    DisplayName = "Demo-PM1",
                }, "DemoPassword321!");
            }
            if (!context.Users.Any(u => u.Email == "demoadmin@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "demoadmin@mailinator.com",
                    Email = "demoadmin@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Admin",
                    DisplayName = "Demo-Admin1",
                }, "DemoPassword321!");
            }




            //Assign users to roles (me to admin, jason to mod)
            var adminId = userManager.FindByEmail("eli_frazier@yahoo.com").Id;
            userManager.AddToRole(adminId, "Admin");

            var demoSub = userManager.FindByEmail("demosub@mailinator.com").Id;
            userManager.AddToRole(demoSub, "Submitter");

            var demoDev = userManager.FindByEmail("demodev@mailinator.com").Id;
            userManager.AddToRole(demoDev, "Developer");

            var demoPm = userManager.FindByEmail("demopm@mailinator.com").Id;
            userManager.AddToRole(demoPm, "Project Manager");

            var demoAdmin = userManager.FindByEmail("demoadmin@mailinator.com").Id;
            userManager.AddToRole(demoAdmin, "Admin");




            //seed priorties and such
            context.TicketPriorities.AddOrUpdate(p => p.Name,
                new TicketPriority { Name = "High"},
                new TicketPriority { Name = "Medium" },
                new TicketPriority { Name = "Low" },
                new TicketPriority { Name = "Optional" }
            );

            context.TicketTypes.AddOrUpdate(t => t.Name,
                new TicketType { Name = "Bug" },
                new TicketType { Name = "Documentation" },
                new TicketType { Name = "New Request" }
            );

            context.TicketStatus.AddOrUpdate(s => s.Name,
                new TicketStatus { Name = "Active" },
                new TicketStatus { Name = "Finished" },
                new TicketStatus { Name = "Assigned" },
                new TicketStatus { Name = "Optional" }
            );
        }
    }
}
