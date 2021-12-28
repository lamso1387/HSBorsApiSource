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
using System.ComponentModel;

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

        [DisplayName("جستجوی صندوق")]
        [HttpPost("search")]
        public async Task<IActionResult> SearchFund([FromBody] SearchFundRequest request)
        {
            string method = nameof(SearchFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var query = await DbContext.GetFunds(request).Paging(response, request.page_start, request.page_size)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.name
                    });
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
                LogHandler.LogMethod(EventType.Operation, Logger, method, response);
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);

        }

        [DisplayName("افزودن صندوق")]
        [HttpPost("add")]
        public async Task<IActionResult> AddFund([FromBody] AddFundRequest request)
        {
            string method = nameof(AddFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var fund = request.ToEntity();

                var existingEntity = await DbContext.GetFund(fund);
                if (existingEntity != null)
                {
                    response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                DbContext.Add(fund);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
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
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }

        [DisplayName("ویرایش صندوق")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditFund([FromBody] AddFundRequest request)
        {
            string method = nameof(EditFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = request.ToEntity();
                entity.id = request.id;

                var existingEntity = await DbContext.GetFund(new Fund { id = entity.id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                if (existingEntity.name != entity.name)
                {
                    var existingName = await DbContext.GetFund(new Fund { name = entity.name });
                    if (existingName != null)
                    {
                        response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                        return response.ToHttpResponse(Logger,Request.HttpContext);
                    }
                }

                existingEntity.name = entity.name;
                existingEntity.no = entity.no;
                existingEntity.status = entity.status;

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
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
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }

        [HttpDelete("delete/{id}")]
        [DisplayName("حذف صندوق")]
        public async Task<IActionResult> DeleteFund(long id)
        {
            string method = nameof(DeleteFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            { 
                var existingEntity = await DbContext.GetFund(new Fund { id = id });
                bool single_delete = existingEntity.DeleteSingleFund(DbContext, ref response);
                if (single_delete == false)
                    return response.ToHttpResponse(Logger,Request.HttpContext); 

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

        [HttpDelete("delete/group")]
        [DisplayName("حذف گروه صندوق")]
        public async Task<IActionResult> DeleteGroupFund([FromBody] List<long> ids)
        {
            string method = nameof(DeleteGroupFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, ids);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                foreach (var id in ids)
                {
                    var existingEntity =await DbContext.GetFund(new Fund { id = id });
                    bool single_delete = existingEntity.DeleteSingleFund(DbContext,ref response);
                    if(single_delete==false)
                        return response.ToHttpResponse(Logger,Request.HttpContext);
                }  
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

        [HttpGet("{id}")]
        [DisplayName("مشاهده صندوق")]
        public async Task<IActionResult> GetFund(long id)
        {
            string method = nameof(GetFund);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetFund(new Fund { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                DbContext.Entry(existingEntity).Collection(x => x.unit_costs).Load();
                response.Model = new List<Fund> { existingEntity }.Select(x => new
                {
                    x.create_date,
                    x.creator_id,
                    x.final_cancel_cost,
                    x.final_cancel_cost_date,
                    x.id,
                    x.name,
                    x.no,
                    x.status,
                }).First();
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }

        [DisplayName("جستجوی داشبورد صندوق")]
        [HttpPost("dashboard/search")]
        public async Task<IActionResult> SearchFundDashboard([FromBody] SearchFundRequest request)
        {
            string method = nameof(SearchFundDashboard);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var fund_cancel_cost = DbContext.GetUnitcosts();

                var funds = DbContext.GetFunds();
                var uncomplete_fund_data = funds.Where(x => !fund_cancel_cost.Select(z => z.fund_id).Distinct().Contains(x.id));
                if (await uncomplete_fund_data.AnyAsync())
                {
                    var uncomplete_fund = await uncomplete_fund_data.FirstAsync();
                    response.ErrorData = Newtonsoft.Json.JsonConvert.SerializeObject(
                        new Dictionary<string, object>
                        { [nameof(Deposit.fund_id)] = uncomplete_fund.id, [nameof(Deposit.fund_name)] = uncomplete_fund.name });
                    response.ErrorCode = (int)ErrorCode.FundCancelCostNotSet;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                decimal all_fund_total_amount = (await funds.Include(x => x.accounts).ThenInclude(x => x.deposits)
                 .ToListAsync()).Sum(x => x.total_amount);

                var all_funds = await funds
                  .Include(x => x.accounts).ThenInclude(x => x.deposits).ThenInclude(x => x.unit_cost)
                .ToListAsync();

                decimal all_balanced_profit_average_percent = all_funds.Sum(x => x.total_amount / all_fund_total_amount * x.interest_rate);

                var query = (await fund_cancel_cost
                    .Include(x => x.deposits).ThenInclude(x => x.purchases)
                    .Include(x => x.fund).ThenInclude(x => x.accounts).ThenInclude(x => x.deposits)
                    .ToListAsync())
                    .GroupBy(x => new { x.fund_id })
                    .Select(x => new
                    {
                        x.First().fund_name,
                        x.First().fund_id,
                        total_amount = x.SelectMany(y => y.deposits.SelectMany(c => c.purchases.Where(u => !request.buyer_id.HasValue || u.buyer_id == request.buyer_id))).Sum(b => b.amount),
                        x.First().fund.final_cancel_cost_date,
                        x.First().fund.final_cancel_cost,
                        count = x.Sum(y => y.deposits.Sum(c => c.purchases
                        .Where(u => !request.buyer_id.HasValue || u.buyer_id == request.buyer_id)
                        .Sum(r => r.deposit_ratio * c.count))),
                        x.First().fund.interest_rate,
                        balanced_profit_average_percent = (x.First().fund.total_amount / all_fund_total_amount * x.First().fund.interest_rate),
                        profit = x.Sum(y => y.deposits.Sum(c => c.profit_rate)),
                        total_share = (x.First().fund.total_amount / all_fund_total_amount),
                        profit_to_investment = x.SelectMany(y => y.deposits).Select(z => z.daily_profit_returns).DefaultIfEmpty(0).Average()

                    });
                var entity_list = query
                    .Where(x => x.count > 0)
                    .Select(x => new
                    {
                        name = x.fund_name,
                        x.fund_id,
                        x.total_amount,//هزینه خرید واحدهای صندوق 
                        capital_value = x.count * x.final_cancel_cost,//ارزش سرمایه صندوق
                        interest_rate = x.interest_rate.ToString("0.##"),//درصد سود صندوق
                        balanced_profit_average_percent = x.balanced_profit_average_percent.ToString("0.##"),//میانگین درصد سود موزون صندوق
                        x.final_cancel_cost_date,
                        x.profit,//میزان سود صندوق
                        all_fund_total_amount,
                        x.total_share,//سهم از کل
                        profit_share = all_balanced_profit_average_percent == 0 ? "0" : (x.balanced_profit_average_percent / all_balanced_profit_average_percent).ToString("0.##"),//سهم از سود
                        profit_to_earnings_ratio = x.total_share == 0 || all_balanced_profit_average_percent == 0 ? "0" :
                        ((x.balanced_profit_average_percent / all_balanced_profit_average_percent) / x.total_share).ToString("0.##"),//نسبت سود به سهم
                        profit_to_investment = x.profit_to_investment.ToString("0.##")//نسبت سود به مدت سرمایه‌گذاری
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