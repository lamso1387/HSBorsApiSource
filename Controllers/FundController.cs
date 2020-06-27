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
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Immutable;

namespace HSBors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundController : DefaultController
    {
        public FundController(IDistributedCache distributedCache, ILogger<FundController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [HttpPost("search")] 
        public async Task<IActionResult> SearchFund([FromBody] SearchFundRequest request)
        {
            string method = nameof(SearchFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            { 
                var query = await DbContext.GetFunds(request).Paging(response,request.page_start, request.page_size) 
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.name
                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
                LogHandler.LogMethod(LogHandler.EventType.Operation, Logger, method, response);
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger, method);

        }

        [HttpPost("add")] 
        public async Task<IActionResult> AddFund([FromBody] AddFundRequest request)
        {
            string method = nameof(AddFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var fund = request.ToEntity();

                var existingEntity = await DbContext.GetFund(fund);
                if (existingEntity != null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.AddRepeatedEntity;
                    return response.ToHttpResponse(Logger, method);
                }

                DbContext.Add(fund);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                var entity_list = new List<Fund> { fund }
                    .Select(x => new
                    {
                        x.id,
                        x.status,
                        x.create_date,
                        x.creator_id,
                        x.name,
                        x.no
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

        [HttpPut("edit")]
        public async Task<IActionResult> EditFund([FromBody] AddFundRequest request)
        {
            string method = nameof(EditFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var entity = request.ToEntity();
                entity.id = request.id;

                var existingEntity = await DbContext.GetFund(entity);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }

                existingEntity.name = entity.name;
                existingEntity.no = entity.no;
                existingEntity.status = entity.status;

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                var entity_list = new List<Fund> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.status,
                        x.create_date,
                        x.creator_id,
                        x.name,
                        x.no
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
        public async Task<IActionResult> DeleteFund(long id)
        {
            string method = nameof(DeleteFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetFund(new Fund { id = id });
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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFund(long id)
        {
            string method = nameof(GetFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetFund(new Fund { id = id });
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

        [HttpPost("dashboard/search")]
        public async Task<IActionResult> DashboardSearchFund([FromBody] SearchFundRequest request)
        {
            string method = nameof(DashboardSearchFund);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var fund_cancel_cost = DbContext.GetUnitcosts();

                var uncomplete_fund_data = DbContext.GetFunds()
                    .Where(x => !fund_cancel_cost.Select(z => z.fund_id).Distinct().Contains(x.id));
                if (await uncomplete_fund_data.AnyAsync())
                {
                    var uncomplete_fund = await uncomplete_fund_data.FirstAsync();
                    response.ErrorData = Newtonsoft.Json.JsonConvert.SerializeObject(
                        new Dictionary<string, object>
                        { [nameof(Deposit.fund_id)] = uncomplete_fund.id, [nameof(Deposit.fund_name)] = uncomplete_fund.name });
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.FundCancelCostNotSet;
                    return response.ToHttpResponse(Logger, method);
                }
                var all_funds =DbContext.GetFunds().Include(x => x.accounts).ThenInclude(x => x.deposits)
                    .Include(x => x.unit_costs).ThenInclude(x=>x.deposits).
                    ToList();
                decimal all_fund_total_amount = (decimal)all_funds.Sum(x => x.total_amount);
                decimal all_balanced_profit_average_percent = (decimal)all_funds.
                Sum(x => x.total_amount/ all_fund_total_amount * x.interest_rate);

                LogHandler.LogMethod(LogHandler.EventType.Operation, Logger, method, all_balanced_profit_average_percent);

                var query = (await fund_cancel_cost
                    .Include(x => x.fund)
                    .Include(x => x.deposits).ThenInclude(x => x.account).ThenInclude(x => x.fund)
                    .GroupBy(x => new { x.fund_id }).ToListAsync())
                    .Select(x => new
                    {
                        x.First().fund_name,
                        x.First().fund.total_amount,
                        x.First().fund.final_cancel_cost_date,
                        x.First().fund.final_cancel_cost,
                        count = x.Sum(y => y.deposits.Sum(c => c.count)),
                        x.First().fund.interest_rate,
                        balanced_profit_average_percent = ((decimal?)x.First().fund.total_amount / all_fund_total_amount * x.First().fund.interest_rate),
                        profit = x.Sum(y => y.deposits.Sum(c => c.profit_rate)),
                        total_share = (x.First().fund.total_amount / all_fund_total_amount),
                        profit_to_investment = x.Sum(y => y.deposits.Sum(c => c.daily_profit_returns))
                    });
                var entity_list = query
                    .Select(x => new
                    {
                        name = x.fund_name,
                        x.total_amount,
                        capital_value = x.count * x.final_cancel_cost,//ارزش سرمایه صندوق
                        interest_rate =x.interest_rate?.ToString("0.##"),
                        balanced_profit_average_percent=x.balanced_profit_average_percent?.ToString("0.##"),//میانگین درصد سود موزون صندوق
                        x.final_cancel_cost,
                        x.final_cancel_cost_date,
                        x.profit,//میزان سود صندوق
                        all_fund_total_amount,
                        total_share = x.total_share?.ToString("0.##"),//سهم از کل
                        profit_share = (x.balanced_profit_average_percent / all_balanced_profit_average_percent)?.ToString("0.##"),//سهم از سود
                        profit_to_earnings_ratio =x.total_share==0? "0":
                        ((x.balanced_profit_average_percent / all_balanced_profit_average_percent) / x.total_share)?.ToString("0.##"),//نسبت سود به سهم
                        profit_to_investment=x.profit_to_investment?.ToString("0.##")//نسبت سود به مدت سرمایه‌گذاری

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