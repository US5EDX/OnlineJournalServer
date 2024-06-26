using DataAccessLayer;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class LogController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(string date)
        {
            try
            {
                DateTime logsDate = DateTime.Parse(date);

                using (OnlineJournalLogsEntities entities = new OnlineJournalLogsEntities())
                {
                    var logs = entities.ActionLogs.Where(log => DbFunctions.TruncateTime(log.date_time) == DbFunctions.TruncateTime(logsDate.Date))
                        .Select(log => new
                        {
                            ActionDateTime = log.date_time,
                            UserEmail = log.user_email,
                            ActionType = log.action_type,
                            log.description,
                            Changes = log.sql_command
                        }).ToList();

                    if (logs != null)
                    {
                        return Ok(logs);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, "Logs not found");
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
