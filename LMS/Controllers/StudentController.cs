﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Student")]
  public class StudentController : CommonController
  {

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Catalog()
    {
      return View();
    }

    public IActionResult Class(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }


    public IActionResult ClassListings(string subject, string num)
    {
      System.Diagnostics.Debug.WriteLine(subject + num);
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }


    /*******Begin code to modify********/

    /// <summary>
    /// Returns a JSON array of the classes the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester
    /// "year" - The year part of the semester
    /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {

            var query = from s in db.Student
                        where s.UId == uid // to check
                        join e in db.Enrolled on s.UId equals e.UId
                        join c in db.Class on e.ClaId equals c.ClassId
                        join cour in db.Course on c.CourseId equals cour.CourseId
                        select new
                        {
                            subject = s.Subject,
                            number = cour.Number,
                            name = cour.Name,
                            season = c.Season,
                            year = c.Year,
                            grade = e.Grade == -1 ? "--" : e.Grade.ToString() // to check
                        };

            return Json(query.ToArray());
        }

    /// <summary>
    /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The category name that the assignment belongs to
    /// "due" - The due Date/Time
    /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="uid"></param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
    {

            var query1 = from c in db.Course // to check
                         where c.Subject == subject && c.Number == num
                         join clas in db.Class on c.CourseId equals clas.CourseId
                         where clas.Season == season && clas.Year == year
                         join cat in db.AsgnCategory on clas.ClassId equals cat.ClaId
                         join a in db.Assignment on cat.CatId equals a.CatId
                         select a;

            var query2 = from q in query1
                         join s in db.Submission on
                         new { A = q.AId, B = uid } equals new { A = s.AId, B = s.UId }
                         into join1
                         from j in join1.DefaultIfEmpty()
                         select new
                         {
                             aname = q.Name,
                             cname = q.Cat.Name,
                             due = q.Due,
                             score = j == null ? null : (int?)j.Score
                         };

            return Json(query2.ToArray());
        }



    /// <summary>
    /// Adds a submission to the given assignment for the given student
    /// The submission should use the current time as its DateTime
    /// You can get the current time with DateTime.Now
    /// The score of the submission should start as 0 until a Professor grades it
    /// If a Student submits to an assignment again, it should replace the submission contents
    /// and the submission time (the score should remain the same).
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="uid">The student submitting the assignment</param>
    /// <param name="contents">The text contents of the student's submission</param>
    /// <returns>A JSON object containing {success = true/false}</returns>
    public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, 
      string category, string asgname, string uid, string contents)
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

            if (query.Count() == 0)
            {
                return Json(new { success = false });
            }

            Submission sub = new Submission();
            sub.UId = uid;
            sub.AId = query.First().AId; // to check
            sub.Contents = contents;
            sub.Score = 0;
            sub.Time = DateTime.Now;

            db.Add(sub);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

    
    /// <summary>
    /// Enrolls a student in a class.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing {success = {true/false}. 
    /// false if the student is already enrolled in the class, true otherwise.</returns>
    public IActionResult Enroll(string subject, int num, string season, int year, string uid)
    {

            var query = from c in db.Course
                        where c.Subject == subject && c.Number == num
                        join clas in db.Class on c.CourseId equals clas.CourseId
                        where clas.Season == season && clas.Year == year
                        join e in db.Enrolled on clas.ClassId equals e.ClaId
                        where e.UId == uid
                        select clas;

            if (query.Count() != 0)
            {
                return Json(new { success = false });
            }

            var query2 = from c in db.Course
                         where c.Subject == subject && c.Number == num
                         join clas in db.Class on c.CourseId equals clas.CourseId
                         where clas.Season == season && clas.Year == year
                         select clas;

            Enrolled en = new Enrolled();
            en.ClaId = query2.First().ClassId;
            en.UId = uid;
            en.Grade = -1;
            db.Add(en);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }



    /// <summary>
    /// Calculates a student's GPA
    /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
    /// Assume all classes are 4 credit hours.
    /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
    /// If a student is not enrolled in any classes, they have a GPA of 0.0.
    /// Otherwise, the point-value of a letter grade is determined by the table on this page:
    /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
    public IActionResult GetGPA(string uid)
    {

            var query = from e in db.Enrolled
                        where e.UId == uid
                        select e.Grade;

            int classCount = 0;
            double GPA = 0;

            foreach (var grade in query)
            {
                if (grade == -1)
                    continue;

                classCount += 1;

                if (GPA >= 93)
                    GPA = 4.0;
                else if (GPA >= 90)
                    GPA = 3.7;
                else if (GPA >= 87)
                    GPA = 3.3;
                else if (GPA >= 83)
                    GPA = 3.0;
                else if (GPA >= 80)
                    GPA = 2.7;
                else if (GPA >= 77)
                    GPA = 2.3;
                else if (GPA >= 73)
                    GPA = 2.0;
                else if (GPA >= 70)
                    GPA = 1.7;
                else if (GPA >= 67)
                    GPA = 1.3;
                else if (GPA >= 63)
                    GPA = 1.0;
                else if (GPA >= 60)
                    GPA = 0.7;
                else
                    GPA = 0;


            }

            return Json(new { gpa = GPA / classCount });
    }

    /*******End code to modify********/

  }
}