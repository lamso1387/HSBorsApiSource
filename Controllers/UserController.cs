using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HSBors.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using Microsoft.Build.Execution;
using System.ComponentModel;
using SRL;
using System.Net.Http;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using System.Text;
using HSBors.Services;
using HSBors.Middleware;
//using Microsoft.EntityFrameworkCore.Internal; 

namespace HSBors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : DefaultController
    {
        private UserService _userService;


        public UserController(IDistributedCache distributedCache, ILogger<UserController> logger, HSBorsDb dbContext, UserService userService) :
            base(distributedCache, logger, dbContext)
        {
            _userService = userService;

            LogHandler.LogMethod(EventType.Operation, Logger, "ClaimTypes :", ClaimTypes.NameIdentifier);

        }
         
        [HttpPost("authenticatepost")]
        [DisplayName("احراز هویت")]
        public async Task<IActionResult> AuthenticatePost([FromBody] User user)
        {
            string method = nameof(AuthenticatePost);
            LogHandler.LogMethod(EventType.Call, Logger, method, user);
            SingleResponse<object> response = new SingleResponse<object>();
            try
            {
                user = await _userService.Authenticate(user?.national_code, user?.password);

                if (user == null)
                {
                    response.ErrorCode = (int)ErrorCode.Unauthorized;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                
                UserSession.SetAccesses(DbContext);

                response.Model = UserSession.Session;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [HttpGet("authenticate")]
        [DisplayName("احراز هویت")]
        public async Task<IActionResult> Authenticate()
        {
            string method = nameof(Authenticate);
            LogHandler.LogMethod(EventType.Call, Logger, method);
            PagedResponse<object> response = new PagedResponse<object>();
            try
            {
                response.Model = UserSession.Accesses;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);

        }
         
        [HttpPost("search")]
        [DisplayName("جستجوی کاربر")]
        public async Task<IActionResult> SearchUser([FromBody] SearchUserRequest request)
        {
            string method = nameof(SearchUser);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {

                var query = await DbContext.GetUsers(request).Paging(response, request.page_start, request.page_size)
                    //  .Include(x=>x.fund)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.first_name,
                        x.last_name,
                        x.full_name,
                        x.mobile,
                        x.national_code
                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);

        }
         
        [HttpPost("add")]
        [DisplayName("افزودن کاربر")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
        {
            string method = nameof(AddUser);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);

            SingleResponse<object> response = new SingleResponse<object>();

            try
            {

                request.pass_mode = PassMode.add;
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var user = request.ToEntity();

                var existingEntity = await DbContext.GetUser(user);
                if (existingEntity != null)
                {
                    response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                user.UpdatePasswordHash(DbContext);

                await DbContext.AddAsync(user);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<User> { user }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.first_name,
                        x.last_name,
                        x.mobile,
                        x.status,
                        x.national_code
                    }).First();
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [DisplayName("حذف کاربر")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            string method = nameof(DeleteUser);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetUser(new User { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                DbContext.Remove(existingEntity);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                response.Model = true;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [DisplayName("ویرایش کاربر")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditUser([FromBody] AddUserRequest request)
        {
            string method = nameof(EditUser);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {

                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = request.ToEntity();
                entity.id = request.id;

                var existingEntity = await DbContext.GetUser(entity);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                existingEntity.first_name = entity.first_name;
                existingEntity.last_name = entity.last_name;
                existingEntity.mobile = entity.mobile;
                existingEntity.national_code = entity.national_code;
                existingEntity.password = entity.password;
                existingEntity.status = entity.status;

                existingEntity.UpdatePasswordHash(DbContext);

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<User> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.first_name,
                        x.last_name,
                        x.mobile,
                        x.national_code,
                        x.status
                    }).First();
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [HttpGet("{id}")]
        [DisplayName("مشاهده کاربر")]
        public async Task<IActionResult> GetUser(long id)
        {
            string method = nameof(GetUser);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();


            try
            { 

                var existingEntity = await DbContext.GetUser(new User { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity = new List<User> { existingEntity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.first_name,
                        x.last_name,
                        x.mobile,
                        x.national_code,
                        x.status
                    }).First();
                response.Model = entity;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [DisplayName("جستجوی داشبورد سود و زیان کاربر")]
        [HttpPost("dashboard/profitloss/search")]
        public async Task<IActionResult> SearchUserProfitLostDashboard([FromBody] SearchUserRequest request)
        {
            string method = nameof(SearchUserProfitLostDashboard);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var user_second_conventions = DbContext.GetConventions();
                var user_second_conventions_query = (await user_second_conventions
                    .Include(x => x.purchases).ThenInclude(x => x.deposit).ThenInclude(x => x.unit_cost).ThenInclude(x => x.fund).ThenInclude(x => x.unit_costs)
                 .ToListAsync())
                    .GroupBy(x => x.second_user_id)
                    .Select(x => new
                    {
                        x.First().second_user_id,
                        partner_share = x.Sum(z => z.purchases.Sum(e => e.partner_share))//سهم شراکت کاربر در سایر خریدهای دیگران
                    });

                var user_purchases = DbContext.GetPurchases(null, null, null, request.id);
                var query = (await user_purchases
                    .Include(x => x.deposit).ThenInclude(x => x.unit_cost).ThenInclude(x => x.fund).ThenInclude(x => x.unit_costs)
                    .Include(x => x.buyer)
                    .Include(x => x.convention).ToListAsync())
                    .GroupBy(x => new { x.buyer_id })
                    .Select(x => new
                    {
                        x.First().buyer_id,
                        x.First().buyer.first_name,
                        x.First().buyer.last_name,
                        total_amount_investment = x.Sum(y => y.amount),
                        current_investment_value = x.Sum(y => y.value_of_shares),
                        net_profit_to_loss_share_without_commission = x.Sum(y => y.profit_rate),
                        expected_bank_interest = x.Sum(z => z.expected_bank_interest),
                        partner_share = x.Sum(y => y.partner_share)//سهمی از خرید که دیگران شریک شده اند
                    })
                    .GroupJoin(user_second_conventions_query, purchase => purchase.buyer_id, conv => conv.second_user_id,
                    (purchase, conv) => new { purchase, conv = conv.SingleOrDefault() });

                var entity_list = query
                    .Select(x => new
                    {
                        x.purchase.buyer_id,
                        x.purchase.first_name,
                        x.purchase.last_name,
                        x.purchase.total_amount_investment,//کل مبلغ سرمایه‌گذاری
                        x.purchase.current_investment_value,//ارزش کنونی سرمایه‌گذاری
                        net_profit_to_loss_share_without_commission = (long)x.purchase.net_profit_to_loss_share_without_commission,//سهم خالص سود به زیان بدون کارمزد
                        expected_bank_interest = (long)x.purchase.expected_bank_interest,//.ToString("0.##"), //سود انتظاری بانکی  
                        remaining_fund_profit = (long)(x.purchase.net_profit_to_loss_share_without_commission + (x.conv?.partner_share ?? 0)
                        - x.purchase.partner_share)  //سهم سود مانده صندوق
                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);

        }
    }
}