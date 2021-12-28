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

    public class MessageResponse : IResponse
    {

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorData { get; set; }
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
     

   
#pragma warning restore CS1591
}