using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HSBors.Models
{
#pragma warning disable CS1591
    public interface IResponse
    {
        int? ErrorCode { get; set; }

        string ErrorMessage { get; set; }
        string ErrorDetail { get; set; }
        string ErrorData { get; set; }
    }

    public interface ISingleResponse<TModel> : IResponse
    {
        TModel Model { get; set; }
    }

    public interface IListResponse<TModel> : IResponse
    {
        IEnumerable<TModel> Model { get; set; }
    }

    public interface IPagedResponse<TModel> : IListResponse<TModel>
    {
        int ItemsCount { get; set; }

        int PageCount { get; }
    }

    public class SingleResponse<TModel> : ISingleResponse<TModel>
    {

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorData { get; set; }

        public TModel Model { get; set; }
    }

    public class ListResponse<TModel> : IListResponse<TModel>
    {
        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorData { get; set; }

        public IEnumerable<TModel> Model { get; set; }
    }

    public class PagedResponse<TModel> : IPagedResponse<TModel>
    {

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorData { get; set; }

        public IEnumerable<TModel> Model { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public int ItemsCount { get; set; }

        public int PageCount
        {
            get
            {
                if (PageSize == 0) return 0;
                return ItemsCount < PageSize ? 1 : (int)(((double)ItemsCount / PageSize) + 1);
            }
        }
    }

    public static class ResponseExtensions
    {
        public static IActionResult ToHttpResponse(this IResponse response, ILogger Logger, string method)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            var value = ToHttp(response, ref status);
            return CreateHttpObject(value, status, Logger, method);

        }

        private static IResponse ToHttp(IResponse response, ref HttpStatusCode status)
        {
            var error = ErrorHandler.ErrorDesc((ErrorHandler.ErrorCode?)response.ErrorCode, response.ErrorMessage);
            status = error.status;
            response.ErrorMessage = error.message;
            return response;
        }
        private static IActionResult CreateHttpObject(IResponse response, HttpStatusCode status, ILogger Logger, string method)
        {
            ObjectResult result = new ObjectResult(response);
            result.StatusCode = (int)status;
            LogHandler.LogMethod(LogHandler.EventType.Return, Logger, method, result);
            return result;
        }


    }

    public class ErrorHandler
    {
        public enum ErrorCode
        {
            OK = 0,
            BadRequest = 1,
            UnexpectedError = 2,
            DbSaveNotDone = 3,
            DbUpdateException = 4,
            AddRepeatedEntity = 5,
            NoContent = 6,
            [Description(@"{""message"":""هزینه ابطال صندوق تعیین نشده است"" ,""status"": ""PreconditionFailed""}")]
            FundCancelCostNotSet = 7,
            [Description(@"{""message"":""سود سالانه تعیین نشده است"" ,""status"": ""PreconditionFailed""}")]
            AnnualProfitNotSet = 8
        }
        public static ErrorProp ErrorDesc(ErrorCode? code, string main_error)
        {
            ErrorProp error = new ErrorProp();
            switch (code)
            {
                case ErrorCode.OK:
                    error.message = Constants.MessageText.OperationOK;
                    error.status = HttpStatusCode.OK;
                    break;
                case ErrorCode.BadRequest:
                    error.message = main_error;
                    error.status = HttpStatusCode.BadRequest;
                    break;
                case ErrorCode.UnexpectedError:
                    error.message = Constants.MessageText.UnexpectedError;
                    error.status = HttpStatusCode.InternalServerError;
                    break;
                case ErrorCode.DbSaveNotDone:
                    error.message = Constants.MessageText.DbSaveNotDone;
                    error.status = HttpStatusCode.ExpectationFailed;
                    break;
                case ErrorCode.DbUpdateException:
                    error.message = Constants.MessageText.DbUpdateException;
                    //error.status = HttpStatusCode.FailedDependency;
                    error.status = HttpStatusCode.UnprocessableEntity;
                    break;
                case ErrorCode.AddRepeatedEntity:
                    error.message = Constants.MessageText.AddRepeatedEntity;
                    error.status = HttpStatusCode.Conflict;
                    break;
                case ErrorCode.NoContent:
                    error.message = Constants.MessageText.NoContent;
                    error.status = HttpStatusCode.NoContent;
                    break;
                case null:
                    break;
                default:
                    error = GetErrorProp((ErrorCode)code);
                    break;
            }
            return error;
        }
        public class ErrorProp
        {
            public string message { get; set; } = Constants.MessageText.ErrorNotSet;
            public HttpStatusCode status { get; set; } = HttpStatusCode.Unused;
        }
        private static ErrorProp GetErrorProp<T>(T key)
        {
            string enum_des_str = SRL.ClassManagement.GetEnumDescription<T>(key);
            ErrorProp enum_des = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorProp>(enum_des_str);
            return enum_des;
        }
    }
#pragma warning restore CS1591
}