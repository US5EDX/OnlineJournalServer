using DataAccessLayer;
using REST_API.Processings.Logging;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class CourseController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string responsible)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var courses = entities.Courses.Include(co => co.Users).Where(co => co.responsible == responsible
                    || co.Users.FirstOrDefault(us => us.email == responsible) != null)
                        .Select(co => new
                        {
                            co.code,
                            co.name,
                            responsible = new
                            {
                                value = co.responsible,
                                display = co.User.fullName
                            },
                            IsCurrnetUserResponsible = co.responsible == responsible
                        }).ToList();

                    if (courses != null)
                    {
                        return Ok(courses);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Courses not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetCoursesForAdmin(string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var courses = entities.Courses.Include(co => co.Users)
                        .Select(co => new
                        {
                            co.code,
                            co.name,
                            responsible = new
                            {
                                value = co.responsible,
                                display = co.User.fullName
                            },
                            AdminCourseRole = co.responsible == email ? 1 : co.Users.FirstOrDefault(us => us.email == email) != null ? 2 : 0
                        }).ToList();

                    if (courses != null)
                    {
                        return Ok(courses);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Courses not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetLecturersInfo(string code)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Include(co => co.Users).FirstOrDefault(co => co.code == code);

                    if (course != null)
                    {
                        var resultObject = new
                        {
                            Responsible = course.User.fullName,
                            Helpers = course.Users.Select(helper => helper.fullName).ToList()
                        };

                        return Ok(resultObject);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Course not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetStudentCoursesInfo(string studentCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var student = entities.Students.Include(st => st.Courses
                    .Select(co => co.Tasks.Select(ta => ta.Marks)))
                        .FirstOrDefault(st => st.code == studentCode);

                    if (student != null)
                    {
                        var courses = student.Courses
                            .Select(co => new
                            {
                                co.code,
                                co.name,
                                Grade = 0,
                                Tasks = co.Tasks.Select(ta => new
                                {
                                    ta.id,
                                    ta.name,
                                    Mark = ta.Marks.Where(ma => ma.studentCode == studentCode)
                                    .Select(ma => ma.mark1)
                                    .DefaultIfEmpty(0).FirstOrDefault()
                                }).ToList()
                            }).ToList();

                        return Ok(courses);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Student not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(string code, string email, [FromBody] Cours updatedCourse)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.FirstOrDefault(co => co.code == code);
                    if (course != null)
                    {
                        course.name = updatedCourse.name;
                        course.responsible = updatedCourse.responsible;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "Course with code " + code + " updated",
                            new object[] { updatedCourse.name, updatedCourse.responsible });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "Course with code " + code + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + code);
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + code + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] Cours newCourse)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    entities.Courses.Add(newCourse);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "Course with code " + newCourse.code + " added",
                            new object[] { newCourse.code, newCourse.name, newCourse.responsible });

                    var res = Request.CreateResponse(HttpStatusCode.Created, newCourse);
                    res.Headers.Location = new Uri(Request.RequestUri + newCourse.code);
                    return res;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string code, string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses
                        .Include(co => co.Students)
                        .Where(co => co.code == code && co.Students.Count == 0).FirstOrDefault();
                    if (course != null)
                    {
                        entities.Courses.Remove(course);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "Course with code " + code + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "Course with code " + code + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + code + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
