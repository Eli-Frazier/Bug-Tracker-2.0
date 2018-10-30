using Bug_Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Helpers
{
    public class UserHelper
    {

        private static ApplicationDbContext db = new ApplicationDbContext();

        //public string GetAvatarPath(string userId)
        //{
        //    var defaultPath = "";

        //    var avatarPath = db.Users.Find(userId).AvatarPath;

        //    if (string.IsNullOrEmpty(avatarPath))
        //        return defaultPath;

        //    return defaultPath;
        //}
    }
}