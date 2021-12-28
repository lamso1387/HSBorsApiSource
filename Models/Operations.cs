using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HSBors.Models
{
    
    public class EnumConvert
    {
        public static T StringToEnum<T>(string str)
        {
            return SRL.Convertor.StringToEnum<T>(str);
        }

    }
    public class HSDate : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true;
            string value_ = value.ToString();
            if (string.IsNullOrWhiteSpace(value_)) return true;
            if (value_.Length != 10) return false;
            if (value_.Substring(4, 1) != "/") return false;
            if (value_.Substring(7, 1) != "/") return false;
            string date = value_.Replace("/", "");
            int year, month, day;
            if (!int.TryParse(date.Substring(0, 4), out year)) return false;
            if (!int.TryParse(date.Substring(4, 2), out month)) return false;
            if (!int.TryParse(date.Substring(6, 2), out day)) return false;
            if (year < 1300) return false;
            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;
            return true;
        }

        public static DateTime ToEnglish(string hsdate)
        {
            string text = hsdate.Replace("/", "");
            int year = int.Parse(text.Substring(0, 4));
            int month = int.Parse(text.Substring(4, 2));
            int day = int.Parse(text.Substring(6, 2));
            DateTime dt = SRL.Convertor.PersianToEnglishDate(year, month, day);
            return dt;
        }
        public static string ToHSDate(DateTime? date)
        {
            string text = SRL.Convertor.EnglishToPersianDate(date);
            return text;
        }
    }
    public class LogHandler
    {
        
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
                    Logger.LogCritical(event_id, ex, $"{event_type.ToString()} {ex.GetType().Name} {method}");
                    break;
                case EventType.Call:
                case EventType.Return:
                case EventType.Operation:
                    Logger.LogInformation(event_id, $"{event_type.ToString()} {method} {string.Join(",", inputs)}");
                    break;

            }
        }
        public static void LogError(ILogger Logger, IResponse response, string method, Exception ex)
        {
            response.ErrorCode = (int)ErrorCode.UnexpectedError;
            response.ErrorDetail = SRL.ActionManagement.Exceptions.CreateExactErrorMessage(ex);
            switch (ex.GetType().Name)
            { 
                case nameof(DbUpdateException):
                    response.ErrorCode = (int)ErrorCode.DbUpdateException; 
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
