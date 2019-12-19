using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;


namespace WeddingPlanner.Controllers
{
    public class WeddingController : Controller
    {
        private MyContext dbContext;
        public WeddingController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {   
            // if the user does not have an id then it gets thrown out
            if(HttpContext.Session.GetInt32("userid") == null)
            {
                return RedirectToAction("RegLog", "User");
            }
            
            // Gets a single wedding from all the weddings then deletes it
            List<Wedding> DeleteWedding = dbContext.Weddings.ToList();
            foreach(Wedding RemoveWedding in DeleteWedding)
            {
                if(RemoveWedding.WeddingDate < DateTime.Now)
                {
                    dbContext.Remove(RemoveWedding);
                    dbContext.SaveChanges();
                }
            }
            
            // Locates a all guests from a wedding list 
            List<Wedding> AllWeddingGuests = dbContext.Weddings.Include(w => w.GuestsAttending).ThenInclude(a => a.AGuest).ToList();
            // GuestsAttending comes from Wedding.cs in Models
            // AGuest comes from Association.cs in Models

            // if a user is logged in and is not the creator then it can be invited to the wedding
            // LoggedUserId is used in Dashboard.cshtml
            int? LoggedUserId = HttpContext.Session.GetInt32("userid");
            List<Wedding> GuestList = new List<Wedding>();
            foreach(Wedding guest in AllWeddingGuests)
            {
                // LoggedUserId comes from line 51 and CreatorId comes from Wedding.cs
                if(LoggedUserId != guest.CreatorId)
                {
                    // to add the user's name on the guest list. 
                    GuestList.Add(guest);
                }
            }

            // this is to pass data to the cshtml because I used two models. I made up a random name of LoggedOn to locate someone who is logged on
            User LoggedOn = dbContext.Users.FirstOrDefault(u => u.UserId == LoggedUserId);
            // ViewBag.firstName will be used in Dashboard.cshtml while FirstName comes from The User.cs file
            // LoggedUserId comes from line 50
            ViewBag.firstName = LoggedOn.FirstName;
            ViewBag.lastName = LoggedOn.LastName;
            ViewBag.LoggedUserId = LoggedUserId;
            return View(AllWeddingGuests);
        }

        [HttpGet("ProcessLoggingOut")]
        public IActionResult ProcessLoggingOut()
        {
            // Log out the logged in user. Clear session and go back to RegLog.cshtml
            HttpContext.Session.GetInt32("userid");
            // Used to clear session
            HttpContext.Session.Clear();
            return RedirectToAction("RegLog", "User");
        }

        [HttpGet("PlanWedding")]
        public IActionResult PlanWedding()
        {
            if(HttpContext.Session.GetInt32("userid") != null)
            {
                return View("PlanWedding");
            }
            return RedirectToAction("RegLog", "User");
        }

        [HttpPost("ProcessPlanWedding")]
        public IActionResult ProcessPlanWedding(Wedding submission)
        {
            // if the user is valid then it can create a wedding
            if(ModelState.IsValid)
            {
                // LoginUser is a random name that I chose to get a user ID
                int? LogUser = HttpContext.Session.GetInt32("userid");

                // CreatorId comes from Wedding.cs file
                submission.CreatorId = (int)LogUser;
                dbContext.Add(submission);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("PlanWedding", submission);
            }
        }

        // [HttpPost("ProcessPlanWedding")]
        // public IActionResult ProcessPlanWedding(Wedding submission)
        // {
        //     // if the user is valid then it can create a wedding
        //     if(ModelState.IsValid)
        //     {
        //         // LoginUser is a random name that I chose to get a user ID
        //         int? LogUser = HttpContext.Session.GetInt32("userid");

        //         // CreatorId comes from Wedding.cs file
        //         submission.CreatorId = (int)LogUser;
        //         dbContext.Add(submission);
        //         dbContext.SaveChanges();
        //         return RedirectToAction($"ProcessJoinWedding/{submission.WeddingId}");
        //     }
        //     else
        //     {
        //         return View("PlanWedding", submission);
        //     }
        // }

        [HttpGet("ViewWedding/{weddingId}")]
        public IActionResult ViewWedding(int weddingId)
        {
            if(HttpContext.Session.GetInt32("userid") == null)
            {
                // if the user is not valid then kick it out
                HttpContext.Session.GetInt32("userid");
                HttpContext.Session.Clear();
                return RedirectToAction("RegLog", "User");
            }

            // from all the weddings created, pick a single wedding to view it. The same code appears on line 125 and 127 also appear on 220 and 221
            dbContext.Weddings.Include(w => w.GuestsAttending).ThenInclude(a => a.AGuest).Where(w => w.WeddingId == weddingId);
            // The code below is almost exactly the same as the code above. ThisWedding is a random word that I chose.
            Wedding ThisWedding = dbContext.Weddings.Include(w => w.GuestsAttending).ThenInclude(a => a.AGuest).FirstOrDefault(w => w.WeddingId == weddingId);
            if(ThisWedding == null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(ThisWedding);
        }

        [HttpGet("/ProcessJoinWedding/{WeddingId}")]
        public IActionResult ProcessJoinWedding(int WeddingId)
        {
            // if the user is not valid then kick it out
            if(HttpContext.Session.GetInt32("userid") == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("RegLog", "User");
            }
            
            // if the user logged in then. LoggedUser is a random word that I chose
            int? LoggedUser = HttpContext.Session.GetInt32("userid");

            // this locates the logged in user. LogUser is a random word
            User LogUser = dbContext.Users.FirstOrDefault(u => u.UserId == LoggedUser);
            // this locates the first or default wedding with a particular ID
            Wedding ThisWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == WeddingId);
            if(ThisWedding == null)
            {
                return RedirectToAction("Dashboard");
            }

            // locates one person who is not the creator to be able be invited to a wedding
            Association NewGuest = new Association();
            
            // enables the logged in user to be invited to a specific wedding
            // NewGuest comes from the Association on line 144 and LogUser comes from line 137
            NewGuest.UserId = LogUser.UserId;
            // WeddingId comes from Wedding.cs and ThisWedding comes from line 142
            NewGuest.WeddingId = ThisWedding.WeddingId;

            // locates the logged in user so it can be able to join a wedding and never to able to join it multiple times
            // I created JoinWeddingUser randomly. It doesn't belong anywhere else besides lines 154 and 155
            int? JoinWeddingUser = HttpContext.Session.GetInt32("userid");
            if(!dbContext.Associations.Any(a => a.UserId == JoinWeddingUser && a.WeddingId == WeddingId))
            {
                dbContext.Associations.Add(NewGuest);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("ProcessUnJoinWedding/{weddingId}")]
        public IActionResult ProcessUnJoinWedding(int weddingId)
        {
            // if the user is not valid then kick it out
            if(HttpContext.Session.GetInt32("userid") == null)
            {
                HttpContext.Session.GetInt32("userid");
                HttpContext.Session.Clear();
                return RedirectToAction("RegLog", "User");
            }
            

            // locates the logged in user and makes it able to unjoin a wedding
            int? LoggedUser = HttpContext.Session.GetInt32("userid");
            // this_Wedding i made randomly. I only use it one lines 178 and 179
            Association this_Wedding = dbContext.Associations.FirstOrDefault(u => u.WeddingId == weddingId && u.UserId == (int) LoggedUser);
            dbContext.Remove(this_Wedding);
            dbContext.SaveChanges();
            if(this_Wedding == null)
            {
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Delete/{weddingId}")]
        public IActionResult Delete(int weddingId)
        {
            // locates a logged in user and if that user is not logged in then...
            // LoggedUser is a random word I chose. I only use it on lines 189 and 190
            int? LoggedUser = HttpContext.Session.GetInt32("userid");
            if(LoggedUser == null)
            {
                return RedirectToAction("RegLog", "User");
            }

            // locates a single wedding to be delete it only if that user is the creator
            // DeleteWedding is a random word I chose. I only use it on lines 197, 198, and 200
            Wedding DeleteWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            if(DeleteWedding == null)
            {
                return RedirectToAction("Dashboard");
            }
            if(DeleteWedding.CreatorId == LoggedUser)
            {
                dbContext.Remove(DeleteWedding);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("EditWedding/{weddingId}")]
        public IActionResult EditWedding(int weddingId)
        {
            // if a user is not logged in then kick it out
            if(HttpContext.Session.GetInt32("userid") == null)
            {
                HttpContext.Session.GetInt32("userid");
                HttpContext.Session.Clear();
                return RedirectToAction("RegLog", "User");
            }

            // from all the weddings created, pick a single wedding so the user can edit it. The same code on lines 120 and 122 in ViewWedding
            dbContext.Weddings.Include(w => w.GuestsAttending).ThenInclude(a => a.AGuest).Where(w => w.WeddingId == weddingId);
            // The code below is almost exactly the same as the code above. ThisWedding is a random word that I chose.
            Wedding ThisWedding = dbContext.Weddings.Include(w => w.GuestsAttending).ThenInclude(a => a.AGuest).FirstOrDefault(w => w.WeddingId == weddingId);
            return View("EditWedding", ThisWedding);

        }

        [HttpPost("EditingWedding/{weddingId}")]
        public IActionResult EditingWedding(int weddingId, Wedding EditWedding)
        {
            // if everything is Ok then allow the user to edit or else not allow it to edit
            if(ModelState.IsValid)
            {
                // WeddingToEdit is a random word I chose  
                Wedding WeddingToEdit = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
                WeddingToEdit.WedderOne = EditWedding.WedderOne;
                WeddingToEdit.WedderTwo = EditWedding.WedderTwo;
                WeddingToEdit.WeddingDate = EditWedding.WeddingDate;
                WeddingToEdit.WeddingAddress = EditWedding.WeddingAddress;
                WeddingToEdit.WeddingCity = EditWedding.WeddingCity;
                WeddingToEdit.WeddingState = EditWedding.WeddingState;
                WeddingToEdit.WeddingZipCode = EditWedding.WeddingZipCode;
                WeddingToEdit.UpdatedAt = DateTime.Now;
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard", WeddingToEdit);
            }
            else
            {
                return View("EditWedding", EditWedding);
            }
        }
    }
}
