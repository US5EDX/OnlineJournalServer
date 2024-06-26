using DataAccessLayer;
using REST_API.Processings.Cryptography;
using REST_API.Processings.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class MarkController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string courseCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses
                        .Include(co => co.Tasks.Select(ta => ta.Marks)).FirstOrDefault(co => co.code == courseCode);

                    if (course != null)
                    {
                        var newCol = course.Tasks.Select(ta => new
                        {
                            Code = ta.id,
                            ta.name,
                            Marks = ta.Marks.Select(ma => new
                            {
                                ma.studentCode,
                                ma.taskId,
                                ma.mark1
                            }).ToList()
                        }).ToList();

                        if (newCol != null)
                        {
                            return Ok(newCol);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "Marks not found");
                        }
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
        public HttpResponseMessage Put(string studentCode, int taskId, string email, [FromBody] int newMark)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var mark = entities.Marks.FirstOrDefault(ma => ma.studentCode == studentCode && ma.taskId == taskId);

                    if (mark != null)
                    {
                        mark.mark1 = newMark;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "Mark with student code " + studentCode + " and task id " + taskId + " updated",
                            new object[] { studentCode, taskId, newMark });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "Mark with student code " + studentCode + " and task id " + taskId + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + studentCode.ToString() + taskId.ToString());
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Mark with student code " + studentCode + " and task id " + taskId + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] Mark newMark)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    entities.Marks.Add(newMark);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "Mark " + newMark.mark1 + " with student code "
                        + newMark.studentCode + " and task id " + newMark.taskId + " added",
                            new object[] { newMark.studentCode, newMark.taskId, newMark.mark1 });

                    var res = Request.CreateResponse(HttpStatusCode.Created, newMark);
                    res.Headers.Location = new Uri(Request.RequestUri.ToString());
                    return res;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string studentCode, int taskId, string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var mark = entities.Marks.FirstOrDefault(ma => ma.studentCode == studentCode && ma.taskId == taskId);
                    if (mark != null)
                    {
                        entities.Marks.Remove(mark);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "Mark with student code " + studentCode + " and task id " + taskId + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "Mark with student code " + studentCode + " and task id " + taskId + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Mark with student code " + studentCode + " and task id " + taskId + " is not found!");
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
