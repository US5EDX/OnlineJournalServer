using DataAccessLayer;
using REST_API.Processings.Cryptography;
using REST_API.Processings.DataBase;
using REST_API.Processings.FilesProcessing;
using REST_API.Processings.OptionsChoosing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace REST_API.Controllers
{
    public class ServiceController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetTableNames()
        {
            try
            {
                List<object> tableNames = new List<object>()
                {
                    new { Id = "Classes", Name = "Групи", IsCheked = false },
                    new { Id = "Tasks", Name = "Завдання", IsCheked = false },
                    new { Id = "Users", Name = "Користувачі", IsCheked = false },
                    new { Id = "Courses", Name = "Курси", IsCheked = false },
                    new { Id = "StudentCourses", Name = "Курси студента", IsCheked = false },
                    new { Id = "Marks", Name = "Оцінки", IsCheked = false },
                    new { Id = "CourseHelpers", Name = "Помчники", IsCheked = false },
                    new { Id = "Students", Name = "Студенти", IsCheked = false }
                };

                return Ok(tableNames);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public IHttpActionResult GetRestoreOptions(int option)
        {
            try
            {
                string path = new OptionChoosing().ChooseRestorePath(option);
                var names = new FileProcessing().GetNames(path);

                return Ok(names);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public IHttpActionResult ArchiveDB(int option)
        {
            var result = new DataBaseActionExecuting().ExecuteArchive(option);

            if (result == null)
                return Ok();
            else
                return Content(HttpStatusCode.BadRequest, result);
        }

        [HttpPost]
        public IHttpActionResult ArchiveOnlineJournalTables([FromBody] List<string> toSave)
        {
            try
            {
                var result = new FileProcessing().ArchiveTables(toSave, "OnlineJournal");

                if (result)
                    return Ok();
                else
                    return InternalServerError();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public IHttpActionResult RestoreDB(int option, [FromBody] string fileName)
        {
            var result = new DataBaseActionExecuting().ExecuteRestore(option, fileName);

            if (result == null)
                return Ok();
            else
                return Content(HttpStatusCode.BadRequest, result);
        }

        [HttpPost]
        public IHttpActionResult RestoreOnlineJournalTable([FromBody] string toRestore)
        {
            try
            {
                var result = new FileProcessing().RestoreTable(toRestore, "OnlineJournal");

                if (result)
                {
                    return Ok();
                }
                else
                    return InternalServerError();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        public IHttpActionResult ClearLogs()
        {
            var result = new DataBaseActionExecuting().ExecuteArchive(2);

            if (result != null)
                Content(HttpStatusCode.BadRequest, result);
            try
            {
                new OnlineJournalLogsEntities().Database.ExecuteSqlCommand("TRUNCATE TABLE [ActionLogs]");
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
