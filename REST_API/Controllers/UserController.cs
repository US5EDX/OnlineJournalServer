using DataAccessLayer;
using REST_API.Processings.Cryptography;
using REST_API.Processings.Logging;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class UserController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var users = entities.Users.Select(user => new
                    {
                        user.email,
                        user.fullName,
                        user.phone,
                        user.role
                    }).ToList();

                    if (users != null)
                    {
                        return Ok(users);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Users not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetFullNames()
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var users = entities.Users.Where(us => us.role != 2).Select(user => new
                    {
                        value = user.email,
                        display = user.fullName
                    }).ToList();

                    if (users != null)
                    {
                        return Ok(users);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Users not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetFullNames(string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var users = entities.Users.Where(us => us.email != email)
                        .Select(user => new
                        {
                            value = user.email,
                            display = user.fullName
                        }).ToList();

                    if (users != null)
                    {
                        return Ok(users);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Users not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult Get(string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var userWithoutPassword = entities.Users.Where(user => user.email == email).Select(user => new
                    {
                        user.email,
                        user.fullName,
                        user.phone,
                        user.role
                    }).FirstOrDefault();

                    if (userWithoutPassword != null)
                    {
                        return Ok(userWithoutPassword);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Users not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(string userEmail, string email, [FromBody] User updatedUser)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var user = entities.Users.Where(us => us.email == userEmail).FirstOrDefault();
                    if (user != null)
                    {
                        user.fullName = updatedUser.fullName;
                        user.phone = updatedUser.phone;
                        user.role = updatedUser.role;

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "User with email " + userEmail + " updated",
                            new object[] { updatedUser.fullName, updatedUser.phone, updatedUser.role });

                        var res = Request.CreateResponse(HttpStatusCode.OK, "User with email " + userEmail + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + userEmail);
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + userEmail + " is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(string userEmail, string oldPassword, string email, [FromBody] string newPassword)
        {
            try
            {
                Cryptography cryptography = new Cryptography();

                oldPassword = cryptography.GetPBKDF2Hash(oldPassword);

                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var user = entities.Users.Where(us => us.email == userEmail
                    && us.password == oldPassword).FirstOrDefault();
                    if (user != null)
                    {
                        user.password = cryptography.GetPBKDF2Hash(newPassword);

                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "UPDATE", "User password with email " + userEmail + " updated");

                        var res = Request.CreateResponse(HttpStatusCode.OK, "User password with email " + userEmail + " updated");
                        res.Headers.Location = new Uri(Request.RequestUri + userEmail);
                        return res;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + userEmail + " and password is not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(string email, [FromBody] User newUser)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    newUser.password = new Cryptography().GetPBKDF2Hash(newUser.password);
                    entities.Users.Add(newUser);
                    entities.SaveChanges();

                    new OnlineJournalLogging().AddLog(email, "INSERT", "User with email " + newUser.email + " added",
                            new object[] { newUser.fullName, newUser.phone, newUser.role });

                    var res = Request.CreateResponse(HttpStatusCode.Created, newUser);
                    res.Headers.Location = new Uri(Request.RequestUri + newUser.email);
                    return res;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string userEmail, string email)
        {
            try
            {
                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var user = entities.Users
                        .Include(u => u.Classes).Include(u => u.Courses).Include(u => u.Courses1)
                        .Where(us => us.email == userEmail && us.role != 3 && us.Classes.Count == 0 && us.Courses.Count == 0
                        && us.Courses1.Count == 0).FirstOrDefault();
                    if (user != null)
                    {
                        entities.Users.Remove(user);
                        entities.SaveChanges();

                        new OnlineJournalLogging().AddLog(email, "DELETE", "User with email " + userEmail + " deleted");

                        return Request.CreateResponse(HttpStatusCode.OK, "User with email " + userEmail + " deleted");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with email " + userEmail + " is not found!");
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
