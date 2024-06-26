using DataAccessLayer;
using REST_API.Processings.AuthentificationToken;
using REST_API.Processings.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class AuthController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string email, string password, bool isNeedToken = false)
        {
            try
            {
                string encryptedPassword = new Cryptography().GetPBKDF2Hash(password);

                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var user = entities.Users.FirstOrDefault(us => us.email == email && us.password == encryptedPassword);
                    if (user != null)
                    {
                        var result = new List<object>() { user.role };
                        if (isNeedToken)
                            result.Add(new Token().GenerateToken(user.email));
                        return Ok(result);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Employee with email: " + email + " not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult Get(string token)
        {
            try
            {
                string email = new Token().ValidateToken(token);

                if (token == null)
                    return Content(HttpStatusCode.NotFound, "Token incorrect");

                using (OnlineJournalEntities entities = new OnlineJournalEntities())
                {
                    var user = entities.Users.FirstOrDefault(us => us.email == email);
                    if (user != null)
                    {
                        return Ok((user.email, user.role));
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "User not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
