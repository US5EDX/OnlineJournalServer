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
    public class TaskController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string courseCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var tasks = entities.Tasks.Where(ta => ta.courseCode == courseCode)
                        .Select(ta => new
                        {
                            ta.id,
                            ta.name,
                        }).ToList();

                    if (tasks != null)
                    {
                        return Ok(tasks);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "tasks not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, string email, [FromBody] string taskName)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var student = entities.Tasks.FirstOrDefault(ta => ta.id == id);
                    if (student != null)
                    {
                        student.name = taskName;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "Task with id " + id + " updated",
                            new object[] { taskName });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "Task with id " + id + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + id.ToString());
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Task with id " + id + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] (string taksName, string courseCode) newTask)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    Task task = new Task() { name = newTask.taksName, courseCode = newTask.courseCode };
                    entities.Tasks.Add(task);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "Task with id " + task.id + " added",
                            new object[] { newTask.taksName, newTask.courseCode });

                    var res = Request.CreateResponse(HttpStatusCode.Created, task);
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
        public HttpResponseMessage Delete(int id, string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var task = entities.Tasks
                        .Include(ta => ta.Marks)
                        .Where(ta => ta.id == id && ta.Marks.Count == 0).FirstOrDefault();
                    if (task != null)
                    {
                        entities.Tasks.Remove(task);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "Task with id " + id + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "Task with id " + id + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Task with id " + id + " is not found!");
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
