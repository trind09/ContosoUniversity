using System.Web.Mvc;
using ContosoUniversity.Core.DAL;
using ContosoUniversity.Core.ViewModels;
using System.Linq;
using System;
using PagedList;
using System.Collections.Generic;

namespace ContosoUniversity.Controllers
{
    public class HomeController : Controller
    {
        private SchoolContext db = new SchoolContext();

        public ActionResult Index()
        {
            return View();
        }

        public ViewResult About(string sortOrder, string currentFilter, string searchString, int? page)
        {
            //Sort
            ViewBag.CurrentSort = sortOrder;
            ViewBag.EnrollmentDateSortParm = String.IsNullOrEmpty(sortOrder) ? "enrollment_date_desc" : "";
            ViewBag.StudentLastNameSortParm = sortOrder == "StudentLastName" ? "student_last_name_desc" : "StudentLastName";
            ViewBag.StudentCountSortParm = sortOrder == "StudentCount" ? "student_count_desc" : "StudentCount";

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
            IQueryable<EnrollmentDateGroup> data = from student in db.Students
                                                   group student by student.EnrollmentDate into dateGroup
                                                   select new EnrollmentDateGroup()
                                                   {
                                                       EnrollmentDate = dateGroup.Key,
                                                       StudentCount = dateGroup.Count(),
                                                       StudentLastName = dateGroup.Select(s => s.LastName),
                                                       StudentID = dateGroup.Select(s => s.ID)
                                                   };

            if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.StudentLastName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "enrollment_date_desc":
                    data = data.OrderByDescending(s => s.EnrollmentDate);
                    break;
                case "StudentLastName":
                    data = data.OrderBy(s => s.StudentLastName.FirstOrDefault());
                    break;
                case "student_last_name_desc":
                    data = data.OrderByDescending(s => s.StudentLastName.FirstOrDefault());
                    break;
                case "StudentCount":
                    data = data.OrderBy(s => s.StudentCount);
                    break;
                case "student_count_desc":
                    data = data.OrderByDescending(s => s.StudentCount);
                    break;
                default:
                    data = data.OrderBy(s => s.EnrollmentDate);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            //IQueryable ToPagedList: https://qiita.com/midori44/items/6702f1f041aa663eae57 
            IPagedList<EnrollmentDateGroup> enrollmentDateGroupPages = data.ToPagedList(pageNumber, pageSize);
            return View(enrollmentDateGroupPages);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}