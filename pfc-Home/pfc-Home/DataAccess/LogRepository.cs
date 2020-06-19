using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Diagnostics.AspNet;
using Google.Cloud.Logging.V2;
using Google.Cloud.Logging.Type;
using Google.Api;

namespace pfc_Home.DataAccess
{
    public class LogRepository
    {
        //Error logging
        public void LogError(Exception message)
        {
            string projectId = "pfc-home";
            string serviceName = "pfc-home-app";
            string version = "4.0.0";
            var exceptionLogger = GoogleExceptionLogger.Create(projectId, serviceName, version);
            exceptionLogger.Log(message);
        }


        //Non-error Logging.
        public void WriteLogEntry(string filename, string link, string user)
        {
            string currentDateTime = DateTime.Now.ToString();
            string logId = "PFC-home-logs";
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName("pfc-home", logId);
            LogEntry logEntry = new LogEntry
            {
                LogName = logName.ToString(),
                Severity = LogSeverity.Info,
                
                TextPayload = $"File name: {filename}; Downloaded by: {user}; File Link: {link};  Download DateTime: {currentDateTime};"
            };

            MonitoredResource resource = new MonitoredResource { Type = "global" };
            client.WriteLogEntries(logName, resource, null, new[] { logEntry });
        }




    }
}