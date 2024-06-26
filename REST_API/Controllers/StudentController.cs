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
    public class StudentController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string groupCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var students = entities.Students.Where(st => st.@class == groupCode)
                        .Select(st => new
                        {
                            st.code,
                            st.name,
                            st.surname,
                            st.patronymic
                        }).ToList();

                    if (students != null)
                    {
                        return Ok(students);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Students not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetWithFullName(string courseCode)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var course = entities.Courses.Include(co => co.Students).Where(co => co.code == courseCode).FirstOrDefault();

                    if (course != null)
                    {
                        var students = course.Students.Select(st => new
                        {
                            st.code,
                            FullName = st.name + ' ' + st.surname + ' ' + st.patronymic
                        }).ToList();

                        if (students != null)
                        {
                            return Ok(students);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "Students not found");
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
        public HttpResponseMessage Put(string code, string email, [FromBody] Student updatedStudent)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var student = entities.Students.FirstOrDefault(st => st.code == code);
                    if (student != null)
                    {
                        student.name = updatedStudent.name;
                        student.surname = updatedStudent.surname;
                        student.patronymic = updatedStudent.patronymic;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "Student with code " + code + " updated",
                            new object[] { updatedStudent.name, updatedStudent.surname, updatedStudent.patronymic });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "Student with code " + code + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + code);
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Student with code " + code + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] Student newStudent)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    entities.Students.Add(newStudent);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "Student with code " + newStudent.code + " added",
                            new object[] { newStudent.code, newStudent.name, newStudent.surname, newStudent.patronymic, newStudent.@class });

                    var res = Request.CreateResponse(HttpStatusCode.Created, newStudent);
                    res.Headers.Location = new Uri(Request.RequestUri + newStudent.code);
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
                    var student = entities.Students
                        .Include(st => st.Courses)
                        .Where(st => st.code == code && st.Courses.Count == 0).FirstOrDefault();
                    if (student != null)
                    {
                        entities.Students.Remove(student);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "Student with code " + code + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "Student with code " + code + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Student with code " + code + " is not found!");
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
