using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HSBors.Models
{
    public enum EntityStatus
    {
        inactive = 0,
        active = 1
    }
    public enum SettingType
    {
        AnnualProfit
    }
    public class EnumConvert
    {
        public static T StringToEnum<T>(string str)
        {
            return SRL.Convertor.StringToEnum<T>(str);
        }
    }
    public class LogHandler
    {
        public enum EventType
        {
            Call = 10,
            Return = 11,
            Exception = 12,
            Operation = 13
        }
        public static void LogMethod(EventType event_type, ILogger Logger, string method, params object[] parmeters)
        {

            Log(event_type, Logger, method, null, parmeters);
        }
        private static void Log(EventType event_type, ILogger Logger, string method, Exception ex, params object[] parmeters)
        {
            List<string> inputs = new List<string>();
            foreach (object item in parmeters)
            {
                if (item != null)
                    inputs.Add(Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented));
                else inputs.Add("null");
            }

            var event_id = new EventId((int)event_type);
            switch (event_type)
            {
                case EventType.Exception:
                    Logger?.LogCritical(event_id, ex, $"{event_type.ToString()} {ex.GetType().Name} {method}");
                    break;
                case EventType.Call:
                case EventType.Return:
                case EventType.Operation:
                    Logger?.LogInformation(event_id, $"{event_type.ToString()} {method} {string.Join(",", inputs)}");
                    break;

            }
        }
        public static void LogError(ILogger Logger, IResponse response, string method, Exception ex)
        {
            response.ErrorCode = (int)ErrorHandler.ErrorCode.UnexpectedError;
            response.ErrorDetail = SRL.ActionManagement.Exceptions.CreateExactErrorMessage(ex);
            switch (ex.GetType().Name)
            { 
                case nameof(DbUpdateException):
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbUpdateException; 
                    break; 
            }

            Log(EventType.Exception, Logger, method, ex);
        }
        public static void CatchFunction(ILogger Logger, IResponse response, string method, Exception ex)
        {
            switch (ex.GetType().Name)
            {
                case nameof(DbUpdateException):

                    break;
                default:
                    LogError(Logger, response, method, ex);
                    break;
            }
        }
    }

}
