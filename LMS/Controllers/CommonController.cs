﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
  public class CommonController : Controller
  {

    /*******Begin code to modify********/

    // TODO: Uncomment and change 'X' after you have scaffoled

    
    protected Team11LMSContext db;

    public CommonController()
    {
      db = new Team11LMSContext();
    }
    

    /*
     * WARNING: This is the quick and easy way to make the controller
     *          use a different LibraryContext - good enough for our purposes.
     *          The "right" way is through Dependency Injection via the constructor 
     *          (look this up if interested).
    */

    // TODO: Uncomment and change 'X' after you have scaffoled

    public void UseLMSContext(Team11LMSContext ctx)
    {
      db = ctx;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
    



    /// <summary>
    /// Retreive a JSON array of all departments from the database.
    /// Each object in the array should have a field called "name" and "subject",
    /// where "name" is the department name and "subject" is the subject abbreviation.
    /// </summary>
    /// <returns>The JSON array</returns>
    public IActionResult GetDepartments()
    {
            // TODO: Do not return this hard-coded array
            var query = from d in db.Department
                        select new { name = d.Name, subject = d.Subject };

            return Json(query.ToArray());
        }



    /// <summary>
    /// Returns a JSON array representing the course catalog.
    /// Each object in the array should have the following fields:
    /// "subject": The subject abbreviation, (e.g. "CS")
    /// "dname": The department name, as in "Computer Science"
    /// "courses": An array of JSON objects representing the courses in the department.
    ///            Each field in this inner-array should have the following fields:
    ///            "number": The course number (e.g. 5530)
    ///            "cname": The course name (e.g. "Database Systems")
    /// </summary>
    /// <returns>The JSON array</returns>
    public IActionResult GetCatalog()
    {

            var query = from d in db.Department
                        join c in db.Course on d.Subject equals c.Subject
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = (from i in d.Course // to check
                                       select new
                                       {
                                           number = i.Number,
                                           cname = i.Name
                                       }
                                       ).ToArray()
                        };

            return Json(query.ToArray());
        }

    /// <summary>
    /// Returns a JSON array of all class offerings of a specific course.
    /// Each object in the array should have the following fields:
    /// "season": the season part of the semester, such as "Fall"
    /// "year": the year part of the semester
    /// "location": the location of the class
    /// "start": the start time in format "hh:mm:ss"
    /// "end": the end time in format "hh:mm:ss"
    /// "fname": the first name of the professor
    /// "lname": the last name of the professor
    /// </summary>
    /// <param name="subject">The subject abbreviation, as in "CS"</param>
    /// <param name="number">The course number, as in 5530</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetClassOfferings(string subject, int number)
    {

            var query = from c in db.Course
                        where c.Number == number && c.Subject == subject
                        join clas in db.Class on c.CourseId equals clas.CourseId
                        join prof in db.Professor on clas.UId equals prof.UId
                        select new
                        {
                            season = clas.Season,
                            year = clas.Year,
                            location = clas.Location,
                            start = clas.Start,
                            end = clas.End,
                            fname = prof.FirstName,
                            lname = prof.LastName
                        };

            return Json(query.ToArray());
        }

    /// <summary>
    /// This method does NOT return JSON. It returns plain text (containing html).
    /// Use "return Content(...)" to return plain text.
    /// Returns the contents of an assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment in the category</param>
    /// <returns>The assignment contents</returns>
    public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
    {

            var query = from c in db.Course
                        where c.Subject == subject && c.Number == num
                        join clas in db.Class on c.CourseId equals clas.CourseId
                        where clas.Season == season && clas.Year == year
                        join cat in db.AsgnCategory on clas.ClassId equals cat.ClaId
                        where cat.Name == category
                        join a in db.Assignment on cat.CatId equals a.CatId
                        where a.Name == asgname
                        select a;

            return Content(query.First().Contents);
        }


    /// <summary>
    /// This method does NOT return JSON. It returns plain text (containing html).
    /// Use "return Content(...)" to return plain text.
    /// Returns the contents of an assignment submission.
    /// Returns the empty string ("") if there is no submission.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment in the category</param>
    /// <param name="uid">The uid of the student who submitted it</param>
    /// <returns>The submission text</returns>
    public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
    {

            var query = from c in db.Course
                        where c.Subject == subject && c.Number == num
                        join clas in db.Class on c.CourseId equals clas.CourseId
                        where clas.Season == season && clas.Year == year
                        join cat in db.AsgnCategory on clas.ClassId equals cat.ClaId
                        where cat.Name == category
                        join a in db.Assignment on cat.CatId equals a.CatId
                        where a.Name == asgname
                        join s in db.Submission on a.AId equals s.AId
                        where s.UId == uid
                        select s;

            if (query.Count() == 0)
            {
                return Content("");
            }

            return Content(query.First().Contents);
        }


    /// <summary>
    /// Gets information about a user as a single JSON object.
    /// The object should have the following fields:
    /// "fname": the user's first name
    /// "lname": the user's last name
    /// "uid": the user's uid
    /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
    ///               If the user is a Professor, this is the department they work in.
    ///               If the user is a Student, this is the department they major in.    
    ///               If the user is an Administrator, this field is not present in the returned JSON
    /// </summary>
    /// <param name="uid">The ID of the user</param>
    /// <returns>
    /// The user JSON object 
    /// or an object containing {success: false} if the user doesn't exist
    /// </returns>
    public IActionResult GetUser(string uid)
    {

            var query = from a in db.Administrator
                        where a.UId == uid
                        select new
                        {
                            fname = a.FName,
                            lname = a.LName,
                            uid = a.UId,
                        };

            if (query.Count() != 0)
            {
                return Json(query.First());
            }

            var query2 = from s in db.Student
                         where s.UId == uid
                         join d in db.Department on s.Subject equals d.Subject
                         select new
                         {
                             fname = s.FName,
                             lname = s.LName,
                             uid = s.UId,
                             department = d.Name
                         };

            if (query2.Count() != 0)
            {
                return Json(query2.First());
            }

            var query3 = from p in db.Professor
                         where p.UId == uid
                         join d in db.Department on p.Subject equals d.Subject
                         select new
                         {
                             fname = p.FirstName,
                             lname = p.LastName,
                             uid = p.UId,
                             department = d.Name
                         };

            if (query3.Count() != 0)
            {
                return Json(query3.First());
            }

            return Json(new { success = false });
        }


    /*******End code to modify********/

  }
}