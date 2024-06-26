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
    public class GroupController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var groups = entities.Classes
                        .Select(gr => new
                        {
                            gr.code,
                            curator = new
                            {
                                value = gr.curator,
                                display = gr.User.fullName
                            }
                        }).ToList();

                    if (groups != null)
                    {
                        return Ok(groups);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Groups not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult Get(string code)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var groups = entities.Classes.Where(gr => gr.code == code)
                        .Select(gr => new
                        {
                            gr.code,
                            gr.curator,
                            gr.User.fullName
                        }).FirstOrDefault();

                    if (groups != null)
                    {
                        return Ok(groups);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Groups not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetWithStudents(string courseCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var groups = entities.Classes.Include(gr => gr.Students.Select(st => st.Courses))
                        .Select(gr => new
                        {
                            gr.code,
                            Students = gr.Students.Select(st => new
                            {
                                st.code,
                                FullName = st.name + " " + st.patronymic + " " + st.surname,
                                IsCheked = st.Courses.FirstOrDefault(co => co.code == courseCode) != null
                            })
                        }).ToList();

                    if (groups != null)
                    {
                        return Ok(groups);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Groups not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetCuratorCourse(string curator)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var group = entities.Classes.FirstOrDefault(gr => gr.curator == curator);

                    if (group != null)
                    {
                        return Ok(group.code);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Group not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(string code, string email, [FromBody] string curator)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var group = entities.Classes.Where(gr => gr.code == code).FirstOrDefault();
                    if (group != null)
                    {
                        group.curator = curator;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "Group with code " + code + " updated",
                            new object[] { curator });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "Group with code " + code + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + code);
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Group with code " + code + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] Class newClass)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    entities.Classes.Add(newClass);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "Group with code " + newClass.code + " added",
                           new object[] { newClass.code, newClass.curator });

                    var res = Request.CreateResponse(HttpStatusCode.Created, newClass);
                    res.Headers.Location = new Uri(Request.RequestUri + newClass.code);
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
                    var group = entities.Classes
                        .Include(gr => gr.Students)
                        .Where(gr => gr.code == code && gr.Students.Count == 0).FirstOrDefault();
                    if (group != null)
                    {
                        entities.Classes.Remove(group);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "Group with code " + code + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "Group with code " + code + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Group with code " + code + " is not found!");
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
