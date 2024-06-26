using DataAccessLayer;
using REST_API.Processings.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class SubscribeController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post(string courseCode, string email, Dictionary<string, bool> updatedCourse)
        {
            try
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    using (OnlineJournalEntities entities = new OnlineJournalEntities())
                    {
                        var course = entities.Courses.Include(co => co.Students).FirstOrDefault(co => co.code == courseCode);
                        if (course != null)
                        {
                            List<ActionLog> logs = new List<ActionLog>();

                            foreach (var pair in updatedCourse)
                            {
                                var student = entities.Students.FirstOrDefault(st => st.code == pair.Key);

                                if (pair.Value == true)
                                {
                                    course.Students.Add(student);

                                    logs.Add(new ActionLog()
                                    {
                                        date_time = TimeDefinition.GesDateTimeNow(),
                                        user_email = email,
                                        action_type = "INSERT",
                                        description = "Student with code "
                                        + student.code + " subscribed from course " + courseCode,
                                        sql_command = $"{courseCode}; student.code"
                                    });
                                }
                                else
                                {
                                    course.Students.Remove(student);

                                    logs.Add(new ActionLog()
                                    {
                                        date_time = TimeDefinition.GesDateTimeNow(),
                                        user_email = email,
                                        action_type = "DELETE",
                                        description = "Student with code "
                                        + student.code + " unsubscribed from course " + courseCode,
                                        sql_command = null
                                    });
                                }
                            }

                            entities.SaveChanges();
                            scope.Complete();

                            new OnlineJournalLogging().AddLogs(logs);

                            var res = Request.CreateResponse(HttpStatusCode.OK, "Course with code " + courseCode + " subscribers updated");
                            res.Headers.Location = new Uri(Request.RequestUri + courseCode);
                            return res;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Course with code " + courseCode + " is not found!");
                        }
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
