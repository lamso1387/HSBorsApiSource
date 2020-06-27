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
namespace HSBors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : DefaultController
    {
        public UserController(IDistributedCache distributedCache, ILogger<UserController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [HttpPost("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchUser([FromBody] SearchUserRequest request)
        {
            string method = nameof(SearchUser);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
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
                        x.mobile
                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger, method);

        }

        [HttpPost("add")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
        {
            string method = nameof(AddUser);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var user = request.ToEntity();

                //var existingEntity = await DbContext.GetUnitcost(unit);
                //if (existingEntity != null)
                //{
                //    response.ErrorCode = (int)ErrorHandler.ErrorCode.AddRepeatedEntity;
                //    return response.ToHttpResponse(Logger, method);
                //}

                DbContext.Add(user);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
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
                        x.username
                    }).First();
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }

        [HttpDelete("delete/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            string method = nameof(DeleteUser);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetUser(new User { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }

                DbContext.Remove(existingEntity);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                response.Model = true;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditUser([FromBody] AddUserRequest request)
        {
            string method = nameof(EditUser);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var entity = request.ToEntity();
                entity.id = request.id;

                var existingEntity = await DbContext.GetUser(entity);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }

                existingEntity.first_name = entity.first_name;
                existingEntity.last_name = entity.last_name;
                existingEntity.mobile = entity.mobile;
                existingEntity.status = entity.status;
                existingEntity.username = entity.username;

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
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
                        x.status,
                        x.username
                    }).First();
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(long id)
        {
            string method = nameof(GetUser);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetUser(new User { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }
                response.Model = existingEntity;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }

        [HttpPost("dashboard/profitloss/search")]
        public async Task<IActionResult> UserProfitLostDashboardSearch()
        {
            string method = nameof(UserProfitLostDashboardSearch);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {

                var user_purchases = DbContext.GetPurchases();

                var query = (await user_purchases
                    .Include(x => x.deposit).ThenInclude(x => x.unit_cost).ThenInclude(x => x.fund)
                    .Include(x => x.buyer)
                    // .Include(x => x.deposits).ThenInclude(x => x.account).ThenInclude(x => x.fund)
                    .GroupBy(x => new { x.buyer_id }).ToListAsync())
                    .Select(x => new
                    {
                        x.First().buyer_id,
                        x.First().buyer.first_name,
                        x.First().buyer.last_name,
                        total_amount_investment = x.Sum(y => y.amount),
                        current_investment_value = x.Sum(y => y.deposit.value_of_shares),
                        net_profit_to_loss_share_without_commission = x.Sum(y => y.deposit.profit_rate),
                        expected_bank_interest = x.Where(y => y.bank_copartner_id == x.First().buyer_id).Sum(z => z.expected_bank_interest)
                    });
                var entity_list = query
                    .Select(x => new
                    {
                        x.buyer_id,
                        x.first_name,
                        x.last_name,
                        x.total_amount_investment,//کل مبلغ سرمایه‌گذاری
                        x.current_investment_value,//ارزش کنونی سرمایه‌گذاری
                        x.net_profit_to_loss_share_without_commission,//سهم خالص سود به زیان بدون کارمزد
                        x.expected_bank_interest //سود انتظاری بانکی

                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger, method);

        }
    }
}