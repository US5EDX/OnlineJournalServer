using System;
using System.Threading;

namespace REST_API.Processings.Logging
{
    public class TimeDefinition
    {
        static private readonly object lockObject = new object();

        public static DateTime GesDateTimeNow()
        {
            lock (lockObject)
            {
                Thread.Sleep(1);

                return DateTime.Now;
            }
        }
    }
}