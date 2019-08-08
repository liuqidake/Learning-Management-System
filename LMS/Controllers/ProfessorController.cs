using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Students(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
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

    public IActionResult Categories(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
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

    public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      ViewData["uid"] = uid;
      return View();
    }

    /*******Begin code to modify********/


    /// <summary>
    /// Returns a JSON array of all the students in a class.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "dob" - date of birth
    /// "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {

            var query = from courses in db.Course
                        join classes in db.Class
                        on courses.CourseId equals classes.CourseId
                        where courses.Subject == subject &&
                        courses.Number == num &&
                        classes.Season == season &&
                        classes.Year == year
                        join enrollments in db.Enrolled
                        on classes.ClassId equals enrollments.ClaId
                        join students in db.Student
                        on enrollments.UId equals students.UId

                        select new
                        {
                            fname = students.FName,
                            lname = students.LName,
                            uid = students.UId,
                            dob = students.Dob,
                            grade = convertToLetterGrade(enrollments.Grade)
                        };

            return Json(query.ToArray());
        }



    /// <summary>
    /// Returns a JSON array with all the assignments in an assignment category for a class.
    /// If the "category" parameter is null, return all assignments in the class.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The assignment category name.
    /// "due" - The due DateTime
    /// "submissions" - The number of submissions to the assignment
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class, 
    /// or null to return assignments from all categories</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
    {
            JsonResult result;
            var query1 = from courses in db.Course
                         join classes in db.Class
                         on courses.CourseId equals classes.CourseId
                         where courses.Subject == subject &&
                         courses.Number == num &&
                         classes.Season == season &&
                         classes.Year == year
                         join categories in db.AsgnCategory
                         on classes.ClassId equals categories.ClaId
                         join assignments in db.Assignment
                         on categories.CatId equals assignments.CatId
                         select new { assignments, categories };

            if (category != null)
            {

                var query2 = from q in query1
                             where q.categories.Name == category
                             select new
                             {
                                 aname = q.assignments.Name,
                                 cname = q.categories.Name,
                                 due = q.assignments.Due,
                                 submissions = (from s in db.Submission where s.AId == q.assignments.AId select s).Count()
                             };

                result = Json(query2.ToArray());

            }
            else
            {

                var query2 = from q in query1
                             select new
                             {
                                 aname = q.assignments.Name,
                                 cname = q.categories.Name,
                                 due = q.assignments.Due,
                                 submissions = (from s in db.Submission where s.AId == q.assignments.AId select s).Count()
                             };

                result = Json(query2.ToArray());
            }

            return result;
        }


    /// <summary>
    /// Returns a JSON array of the assignment categories for a certain class.
    /// Each object in the array should have the folling fields:
    /// "name" - The category name
    /// "weight" - The category weight
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {

            var query = from courses in db.Course
                        join classes in db.Class
                        on courses.CourseId equals classes.CourseId

                        // Match to the appropriate 'Class' instance of the
                        // given course.
                        where courses.Subject == subject &&
                        courses.Number == num &&
                        classes.Season == season &&
                        classes.Year == year

                        // Combine previously joined tables with 'AsgnCategory'
                        // for retrieval of assignment category names to match
                        // with the given category.
                        join categories in db.AsgnCategory
                        on classes.ClassId equals categories.ClaId

                        select new { name = categories.Name, weight = categories.Weight };

            return Json(query.ToArray());
        }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// If a category of the given class with the given name already exists, return success = false.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false} </returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
    {

            var query = from courses in db.Course
                        join classes in db.Class
                        on courses.CourseId equals classes.CourseId
                        where courses.Subject == subject &&
                        courses.Number == num &&
                        classes.Season == season &&
                        classes.Year == year
                        select classes.ClassId;


            AsgnCategory asgnCategory = new AsgnCategory();
            asgnCategory.CatId = db.AsgnCategory.Count() + 1;
            asgnCategory.Name = category;
            asgnCategory.Weight = catweight;
            asgnCategory.ClaId = query.First(); // The query will never be empty.

            db.AsgnCategory.Add(asgnCategory);

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
    /// Creates a new assignment for the given class and category.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="asgpoints">The max point value for the new assignment</param>
    /// <param name="asgdue">The due DateTime for the new assignment</param>
    /// <param name="asgcontents">The contents of the new assignment</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {

            var query1 = from courses in db.Course
                         join classes in db.Class
                         on courses.CourseId equals classes.CourseId

                         where courses.Subject == subject &&
                         courses.Number == num &&
                         classes.Season == season &&
                         classes.Year == year

                         select classes;
            var query2 = from classes in query1
                         join categories in db.AsgnCategory
                         on classes.ClassId equals categories.ClaId
                         where categories.Name == category

                         select categories.CatId;



            Assignment assignment = new Assignment();
            assignment.AId = db.Assignment.Count() + 1;
            assignment.Name = asgname;
            assignment.Contents = asgcontents;
            assignment.Due = asgdue;
            assignment.Points = asgpoints;
            assignment.CatId = query2.First(); // The query will never be empty.

            db.Assignment.Add(assignment);
            var query3 = from classes in query1
                         join enrollments in db.Enrolled
                         on classes.ClassId equals enrollments.ClaId
                         select enrollments.UId;

            try
            {
                db.SaveChanges();


                foreach (string uid in query3)
                {
                    UpdateGrade(subject, num, season, year, uid);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }


    /// <summary>
    /// Gets a JSON array of all the submissions to a certain assignment.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "time" - DateTime of the submission
    /// "score" - The score given to the submission
    /// 
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
    {

            var query = from courses in db.Course
                        join classes in db.Class
                        on courses.CourseId equals classes.CourseId
                        where courses.Subject == subject &&
                        courses.Number == num &&
                        classes.Season == season &&
                        classes.Year == year
                        join categories in db.AsgnCategory
                        on classes.ClassId equals categories.ClaId
                        where categories.Name == category
                        join assignments in db.Assignment
                        on categories.CatId equals assignments.CatId
                        where assignments.Name == asgname
                        join submissions in db.Submission
                        on assignments.AId equals submissions.AId
                        join students in db.Student
                        on submissions.UId equals students.UId

                        select new
                        {
                            fname = students.FName,
                            lname = students.LName,
                            uid = students.UId,
                            time = submissions.Time,
                            score = submissions.Score
                        };

            return Json(query.ToArray());
        }


    /// <summary>
    /// Set the score of an assignment submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <param name="uid">The uid of the student who's submission is being graded</param>
    /// <param name="score">The new score for the submission</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
    {

            var query = from courses in db.Course
                        join classes in db.Class
                        on courses.CourseId equals classes.CourseId
                        where courses.Subject == subject &&
                        courses.Number == num &&
                        classes.Season == season &&
                        classes.Year == year
                        join categories in db.AsgnCategory
                        on classes.ClassId equals categories.ClaId
                        where categories.Name == category
                        join assignments in db.Assignment
                        on categories.CatId equals assignments.CatId
                        where assignments.Name == asgname
                        join submissions in db.Submission
                        on assignments.AId equals submissions.AId
                        where submissions.UId == uid

                        select submissions;



            foreach (Submission s in query)
            {
                s.Score = score;
            }


            try
            {
                db.SaveChanges(); 
                UpdateGrade(subject, num, season, year, uid);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }


    /// <summary>
    /// Returns a JSON array of the classes taught by the specified professor
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester in which the class is taught
    /// "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {

            var query = from classes in db.Class
                        join courses in db.Course
                        on classes.CourseId equals courses.CourseId
                        where classes.UId == uid

                        select new
                        {
                            courses.Subject,
                            courses.Number,
                            courses.Name,
                            classes.Season,
                            classes.Year
                        };

            return Json(query.ToArray());
        }


    /*******End code to modify********/

  }
}