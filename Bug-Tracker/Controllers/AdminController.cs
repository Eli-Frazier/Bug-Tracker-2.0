using Bug_Tracker.Helpers;
using Bug_Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private ProjectHelper projectsHelper = new ProjectHelper();

        // GET: Admin
        public ActionResult RoleAssignment()
        {
            ViewBag.Users = new SelectList(db.Users.ToList(), "Id", "Email");
            ViewBag.Roles = new SelectList(db.Roles.ToList(), "Name", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAssignment(string users, string roles)
        {
            //remove user from role already in
            var currentRoles = roleHelper.ListUserRoles(users);
            if (currentRoles.Count > 0)
            {
                foreach (var role in currentRoles)
                {
                    roleHelper.RemoveUserFromRole(users, role);
                }
            }

            //assign user to role you want them in 
            
             roleHelper.AddUserToRole(users, roles);
            

            //redirect 
            return RedirectToAction("MyProjects", "Projects");
        }

        public ActionResult ProjectAssignment()
        {
            ViewBag.Projects = new SelectList(db.Projects.ToList(), "Id", "Name");
           
            ViewBag.Users = new SelectList (db.Users.ToList(), "Id", "UserName");


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectAssignment(int projects, List<string> users)
        {
            //unassign everyone from selected project
            var projectUsers = projectsHelper.UsersOnProject(projects);
            if(projectUsers.Count > 0)
            {
                foreach(var user in projectUsers.ToList())
                {
                    projectsHelper.RemoveUserFromProject(user.Id, projects);
                }
            }
            //assign selected user to project
            foreach(var userId in users)
            {
                projectsHelper.AddUserToProject(userId, projects);
            }


            //redirect
            return RedirectToAction("Index", "Projects");
        }
    }
}