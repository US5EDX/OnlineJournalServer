using DataAccessLayer;
using System.Collections.Generic;
using System.Transactions;

namespace REST_API.Processings.Logging
{
    public class OnlineJournalLogging
    {
        public bool AddLog(string email, string actionType, string desctiption, object[] objects = null)
        {
            try
            {
                using (OnlineJournalLogsEntities entities = new OnlineJournalLogsEntities())
                {
                    ActionLog actionLog = new ActionLog()
                    {
                        date_time = TimeDefinition.GesDateTimeNow(),
                        user_email = email,
                        action_type = actionType,
                        description = desctiption,
                        sql_command = objects != null ? string.Join("; ", objects) : null
                    };
                    entities.ActionLogs.Add(actionLog);
                    entities.SaveChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddLogs(List<ActionLog> logs)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    using (OnlineJournalLogsEntities entities = new OnlineJournalLogsEntities())
                    {
                        entities.ActionLogs.AddRange(logs);
                        entities.SaveChanges();
                        scope.Complete();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}