using DataAccessLayer;
using REST_API.Models;
using REST_API.Processings.Logging;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class HelperController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string courseCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Where(co => co.code == courseCode)
                        .Include(co => co.Users).FirstOrDefault();

                    if (course != null)
                    {
                        var users = course.Users.Select(us => new
                        {
                            PairItem = new
                            {
                                value = us.email,
                                display = us.fullName
                            }
                        }).ToList();

                        return Ok(users);
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

        [HttpPut]
        public HttpResponseMessage Put(string courseCode, string email, [FromBody] (string oldHelper, string newHelper) helpers)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Where(co => co.code == courseCode).Include(co => co.Users).FirstOrDefault();
                    if (course != null)
                    {
                        var oldUser = entities.Users.FirstOrDefault(us => us.email == helpers.oldHelper);
                        var newUser = entities.Users.FirstOrDefault(us => us.email == helpers.newHelper);

                        if (oldUser != null && newUser != null)
                        {
                            if (course.responsible == newUser.email)
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User with email " + helpers.newHelper + " responsible!");

                            if (course.Users.Contains(newUser))
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User with email " + helpers.newHelper + " already helper!");

                            course.Users.Remove(oldUser);
                            course.Users.Add(newUser);
                            entities.SaveChanges();

                            new OnlineJournalLogging().AddLog(email, "UPDATE", "Course helpers with code " + courseCode + " updated",
                            new object[] { newUser.email, newUser.fullName });

                            var res = Request.CreateResponse(HttpStatusCode.OK, "Course helpers with code " + courseCode + " updated");
                            res.Headers.Location = new Uri(Request.RequestUri + courseCode);
                            return res;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + helpers.oldHelper +
                                "or user with email " + helpers.newHelper + " is not found!");
                        }

                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + courseCode + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] HelperPair newHelper)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Where(co => co.code == newHelper.CourseCode).Include(co => co.Users).FirstOrDefault();

                    if (course != null)
                    {
                        var user = entities.Users.FirstOrDefault(us => us.email == newHelper.Helper);

                        if (user != null)
                        {
                            if (course.responsible == user.email)
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User with email " + newHelper.Helper + " responsible!");

                            course.Users.Add(user);
                            entities.SaveChanges();

                            new OnlineJournalLogging().AddLog(email, "INSERT", "Course with code " + newHelper.CourseCode + " new helper added",
                            new object[] { user.email, user.fullName });

                            var res = Request.CreateResponse(HttpStatusCode.Created, new { CourseCode = newHelper.CourseCode, Helper = newHelper.Helper });
                            res.Headers.Location = new Uri(Request.RequestUri + newHelper.CourseCode);
                            return res;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + newHelper.Helper + " is not found!");
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + newHelper.CourseCode + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string courseCode, string helper, string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Where(co => co.code == courseCode).Include(co => co.Users).FirstOrDefault();

                    if (course != null)
                    {
                        var user = entities.Users.FirstOrDefault(us => us.email == helper);

                        if (user != null)
                        {
                            course.Users.Remove(user);
                            entities.SaveChanges();

                            new OnlineJournalLogging().AddLog(email, "DELETE", "Course with code " + courseCode + " helper " + helper + " deleted");

                            var res = Request.CreateResponse(HttpStatusCode.OK, new { CourseCode = courseCode, Helper = helper });
                            res.Headers.Location = new Uri(Request.RequestUri + courseCode);
                            return res;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + helper + " is not found!");
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + courseCode + " is not found!");
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
