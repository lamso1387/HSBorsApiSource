using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Web;
using System.ComponentModel;
using Task = System.Threading.Tasks.Task;
using System.Net;
using Microsoft.AspNetCore.Builder;
using HSBors.Models;
using HSBors.Services;
using System.Text.Json;
using System.IO;
using System.Globalization;

namespace HSBors.Middleware
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;

        }
        public async Task Invoke(HttpContext context, UserService _userService, ILogger Logger)
        { 
            try
            {
                string action = null;
                bool need_auth = context.NeedAuth(ref action);
                //context.Request.EnableBuffering();
                //using (var reader = new StreamReader(
                //    context.Request.Body,
                //    encoding: Encoding.UTF8,
                //    detectEncodingFromByteOrderMarks: false,
                //    bufferSize: 1000000,
                //    leaveOpen: true))
                //{
                //    var body = await reader.ReadToEndAsync(); 
                //    // LogHandler.LogMethod(EventType.Call, Logger, method, body); 
                //    context.Request.Body.Position = 0;
                //}

                if (need_auth)
                {
                    if (!context.Request.Headers.ContainsKey("Authorization")) throw new UnauthorizedAccessException();
                    User user = null;
                    try
                    {
                        var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
                        var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                        var username = credentials[0];
                        var password = credentials[1];
                        user = await _userService.Authenticate(username, password);
                    }
                    catch
                    {
                        throw new UnauthorizedAccessException();
                    }
                    if (user == null) throw new UnauthorizedAccessException();
                    bool has_authority = false;

                    if (action != "authenticate") has_authority = _userService.Authorization(action);

                    if (has_authority == false) throw new AppForbittenException();

                    var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.full_name)};
                    var identity = new ClaimsIdentity(claims, "BasicAuthentication");
                    var principal = new ClaimsPrincipal(identity);
                    context.User = principal;
                }

                await _next.Invoke(context);
            }

            catch (Exception error)
            {
                // LogHandler.LogError(Logger, response, method, error);
                await  HandleExceptionAsync(context, error);
                return;
               
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception error)
        { 
            string output = string.Empty;
            if (!context.Response.HasStarted)
            {
                string message = error.Message;
                int code = -1;
                switch (error.GetType().Name)
                {
                    case nameof(AppException):
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        code = (int)ErrorCode.BadRequest;
                        break;
                    case nameof(KeyNotFoundException):
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case nameof(UnauthorizedAccessException):
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case nameof(AppForbittenException):
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        message = ErrorProp.GetError(ErrorCode.UnexpectedError).message;
                        break;
                }
                context.Response.ContentType = "application/json";
                output = JsonSerializer.Serialize((new MessageResponse{ ErrorMessage = message,ErrorCode=code })); 
            } 
            return context.Response.WriteAsync(output);
        }
         

    }

    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
    public class AppForbittenException : Exception
    {
    }

    public class ErrorProp
    {
        public string message { get; set; } = Constants.MessageText.ErrorNotSet;
        //  public int code { get; set; } = -1;
        public HttpStatusCode status { get; set; } = HttpStatusCode.Unused;

        public static ErrorProp GetError(ErrorCode key, string message = null)
        {
            string enum_des_str = SRL.ClassManagement.GetEnumDescription(key);
            ErrorProp enum_des = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorProp>(enum_des_str);
            //enum_des.code = (int)(object)key;
            if (message != null) enum_des.message = message;
            return enum_des;
        }
    }
}
