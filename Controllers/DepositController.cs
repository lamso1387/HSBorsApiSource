using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HSBors.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using Microsoft.AspNetCore.Cors;
using System.Net;
using HSBors.Middleware;

namespace HSBors.Controllers
{
#pragma warning disable CS1591

    [ApiController]
    [Route("api/[controller]")]
    public class DepositController : DefaultController
    {
        public DepositController(IDistributedCache distributedCache, ILogger<DepositController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }
        
         
        [DisplayName("تست")]
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            string method = nameof(Test);
            LogHandler.LogMethod(EventType.Call, Logger, method);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var entity_query = DbContext
                    .GetDeposits(HSViewMode.ReadOnly)
                    //.Deposits
                    .Paging(response, 0, 100);
                List<Deposit> entity_list = await entity_query.ToListAsync();
                response.Model = entity_list.Select(x => new
                {
                    x.count
                });
                response.ErrorCode = (int?)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);

        }

        
        [DisplayName("تست بدون اتصال")] 
        [HttpGet("test_no_db")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TestNoDb()
        {
            string method = nameof(TestNoDb);
            LogHandler.LogMethod(EventType.Call, Logger, method);
            PagedResponse<Deposit> response = new PagedResponse<Deposit>();
            response.ErrorCode = 0;
            var rez= response.ToHttpResponse(Logger,Request.HttpContext);
            return rez;

        }
         
        [HttpPost("search")]
        [DisplayName("جستجوی سپرده")]
        public async Task<IActionResult> SearchDeposit([FromBody] SearchDepositRequest request)
        {
            string method = nameof(SearchDeposit);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var query = await DbContext.GetDeposits(HSViewMode.ReadOnly, request).Paging(response, request.page_start, request.page_size)
                    .Include(x => x.unit_cost)
                    .Include(x => x.account).ThenInclude(x => x.fund)
                    .Include(x => x.purchases).ThenInclude(x => x.buyer)
                    .Include(x => x.purchases).ThenInclude(x => x.convention).ThenInclude(x => x.second_user)
                    .ToListAsync();

                var entity_list = query
                    .Select(x => new
                    {
                        x.account_id,
                        x.account_name,
                        x.cancel_cost,
                        x.count,
                        x.date,
                        x.fund_name,
                        x.id,
                        x.issue_cost,
                        x.purchases_count,
                        x.status,
                        x.unit_cost_id,
                        x.amount,
                        purchases = x.purchases
                        .Select(i => new
                        {
                            i.id,
                            i.buyer_id,
                            i.buyer_name,
                            i.bank_copartner_name,
                            buyer =
                        new User { first_name = i.buyer.first_name, last_name = i.buyer.last_name },
                            convention = new Convention { second_user = new User { first_name = i.convention?.second_user?.first_name, last_name = i.convention?.second_user?.last_name } }

                        }),
                        x.fund_id,
                        x.profit_percantage,
                        x.balanced_interest_rate,
                        x.value_of_shares,
                        x.profit_rate,
                        x.investment_day,
                        x.daily_profit_returns

                    }).OrderByDescending(x => x.date);
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
         
        [DisplayName("افزودن سپرده")]
        [HttpPost("add")]
        public async Task<IActionResult> AddDeposit([FromBody] AddDepositRequest request)
        {
            string method = nameof(AddDeposit);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var deposit = request.ToEntity(false);

                var purchases = request.ToPurchaseEntity();

                if (purchases.IsConventionsUnique(DbContext, response) == false) return response.ToHttpResponse(Logger,Request.HttpContext);

                deposit.purchases = purchases;
                if (deposit.PurchasesAmountsOK(response) == false) return response.ToHttpResponse(Logger,Request.HttpContext);
                DbContext.Add(deposit);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                DbContext.Entry(deposit).Reference(x => x.unit_cost).Load();
                var entity_list = new List<Deposit> { deposit }
                    .Select(x => new
                    {
                        x.account_id,
                        x.account_name,
                        x.cancel_cost,
                        x.count,
                        x.date,
                        x.fund_name,
                        x.id,
                        x.issue_cost,
                        x.purchases_count,
                        x.status,
                        x.unit_cost_id,
                        x.amount,
                        x.create_date,
                        x.creator_id
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
         
        [DisplayName("ویرایش سپرده")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditDeposit([FromBody] AddDepositRequest request)
        {
            string method = nameof(EditDeposit);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = request.ToEntity(true);

                var entity_checker = entity;
                entity.purchases = request.ToPurchaseEntity();
                if (entity_checker.PurchasesAmountsOK(response) == false) return response.ToHttpResponse(Logger,Request.HttpContext);

                var existingEntity = await DbContext.GetDeposits(HSViewMode.Edit, null, null, null, request.id)
                    .Include(x => x.purchases).Include(x=>x.unit_cost)
                   . FirstAsync();
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                existingEntity.account_id = entity.account_id;
                existingEntity.status = entity.status;
                existingEntity.unit_cost_id = entity.unit_cost_id;
                existingEntity.status = entity.status;
                existingEntity.count = entity.count;
                existingEntity.amount = entity.amount;



                var purchases_request = request.ToPurchasesRequest(true);
                var purchases_old = DbContext.GetPurchases(null, request.id);
                var purchase_ids_to_delet = purchases_old.Select(x => x.id).Where(x => !purchases_request.Select(y => y.id).Contains(x));
                var purchase_ids_new = purchases_request.Select(x => x.id).Where(x => !purchases_old.Select(y => y.id).Contains(x));

                foreach (var purchase_id_to_delet in purchase_ids_to_delet)
                {
                    var purchase_delet = await DbContext.GetPurchase(new Purchase { id = purchase_id_to_delet });
                    DbContext.Remove(purchase_delet);
                }
                foreach (var purchase_request in purchases_request.Where(x => purchase_ids_new.Distinct().Contains(x.id)))
                {
                    var purchases_new = purchase_request.ToEntity();
                    existingEntity.purchases.Add(purchases_new);
                }

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
               // DbContext.Entry(existingEntity).Reference(x => x.unit_cost).Load();
                var entity_list = new List<Deposit> { existingEntity }
                    .Select(x => new
                    {
                        x.account_id,
                        x.account_name,
                        x.cancel_cost,
                        x.count,
                        x.date,
                        x.fund_name,
                        x.id,
                        x.issue_cost,
                        x.purchases_count,
                        x.status,
                        x.unit_cost_id,
                        x.amount,
                        x.create_date,
                        x.creator_id
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
        [DisplayName("حذف سپرده")]
        public async Task<IActionResult> DeleteDeposit(long id)
        {
            string method = nameof(DeleteDeposit);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetDeposit(new Deposit { id = id });
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
         
        [DisplayName("مشاهده سپرده")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeposit(long id)
        {
            string method = nameof(GetDeposit);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingList = await DbContext.GetDeposits(HSViewMode.ReadOnly, null, null, null, id)
                    .Include(x => x.unit_cost)
                    .Include(x=>x.account).ThenInclude(x=>x.fund)
                    .ToListAsync();

                if (!existingList.Any())
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
               
                var existingEntity = existingList
                    .Select(x => new
                    {
                        x.account_id,
                        x.account_name,
                        x.cancel_cost,
                        x.count,
                        x.date,
                        x.fund_name,
                        x.id,
                        x.issue_cost,
                        x.purchases_count,
                        x.status,
                        x.unit_cost_id,
                        x.amount,
                        x.create_date,
                        x.creator_id,
                        x.fund_id
                    }).First();

                response.Model = existingEntity;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }
    }


#pragma warning restore CS1591
}