using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ContosoUniversity.Core.DAL;
using PagedList;
using ContosoUniversity.Core.Models;

namespace ContosoUniversity.Controllers
{
    public class EnrollmentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Enrollment
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            //Sort
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.LastNameSortParm = sortOrder == "LastName" ? "last_name_desc" : "LastName";
            ViewBag.GradeSortParm = sortOrder == "Grade" ? "grade_desc" : "Grade";

            //Paging
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            //Search
            var enrollments = db.Enrollments.Include(e => e.Course).Include(e => e.Student);
            if (!String.IsNullOrEmpty(searchString))
            {
                enrollments = enrollments.Where(s => s.Course.Title.Contains(searchString)
                                       || s.Student.LastName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "title_desc":
                    enrollments = enrollments.OrderByDescending(s => s.Course.Title);
                    break;
                case "LastName":
                    enrollments = enrollments.OrderBy(s => s.Student.LastName);
                    break;
                case "last_name_desc":
                    enrollments = enrollments.OrderByDescending(s => s.Student.LastName);
                    break;
                case "Grade":
                    enrollments = enrollments.OrderBy(s => s.Grade);
                    break;
                case "grade_desc":
                    enrollments = enrollments.OrderByDescending(s => s.Grade);
                    break;
                default:
                    enrollments = enrollments.OrderBy(s => s.Course.Title);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(enrollments.ToPagedList(pageNumber, pageSize));
        }

        // GET: Enrollment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // GET: Enrollment/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName");
            return View();
        }

        // POST: Enrollment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID, StudentID, Grade")] Enrollment enrollment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Enrollments.Add(enrollment);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var enrollmentToUpdate = db.Enrollments.Find(id);
            if (TryUpdateModel(enrollmentToUpdate, "",
               new string[] { "CourseID", "StudentID", "Grade" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollmentToUpdate.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollmentToUpdate.StudentID);
            return View(enrollmentToUpdate);
        }

        // GET: Enrollment/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Enrollment enrollment = db.Enrollments.Find(id);
                db.Enrollments.Remove(enrollment);
                db.SaveChanges();
            }
            catch (DataException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
