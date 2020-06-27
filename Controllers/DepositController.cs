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

        [HttpGet("test")] 
        public async Task<IActionResult> Test()
        {
            string method = nameof(Test);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var entity_query = DbContext.GetDeposits().Paging(response,0, 100);
                List<Deposit> entity_list = await entity_query.ToListAsync();
                response.Model = entity_list;
                response.ErrorCode = (int?)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);

        }
        [HttpGet("test_no_db")] 
        public async Task<IActionResult> TestNoDb()
        {
            string method = nameof(TestNoDb);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method);
            PagedResponse<Deposit> response = new PagedResponse<Deposit>();

            try
            {
                response.ErrorCode = (int?)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);

        }

        [HttpPost("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchDeposit([FromBody] SearchDepositRequest request)
        {
            string method = nameof(SearchDeposit);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            { 
                var query= await DbContext.GetDeposits(request).Paging(response,request.page_start, request.page_size)
                    .Include(x => x.account).ThenInclude(x => x.fund)
                    .Include(x => x.unit_cost)
                    .Include(x => x.purchases).ThenInclude(x => x.buyer)
                    .Include(x => x.purchases).ThenInclude(x => x.bank_copartner)
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
                        purchases = x.purchases.Select(i => new Purchase { id = i.id, buyer_id = i.buyer_id, buyer = new User { first_name = i.buyer.first_name, last_name = i.buyer.last_name }, bank_copartner = new User { first_name = i.bank_copartner?.first_name, last_name = i.bank_copartner?.last_name } }),
                        x.fund_id
                    }).OrderByDescending(x => x.date);
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
        public async Task<IActionResult> AddDeposit([FromBody] AddDepositRequest request)
        {
            string method = nameof(AddDeposit);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {

                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var deposit = request.ToEntity(false);
                var purchases = request.ToPurchaseEntity();
                if (purchases.Sum(x=>x.amount)!=deposit.amount)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.BadRequest;
                    response.ErrorMessage = Constants.MessageText.PurcheseAmountSumError;
                    return response.ToHttpResponse(Logger, method);
                }
                deposit.purchases = purchases;
                DbContext.Add(deposit);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
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
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditDeposit([FromBody] AddDepositRequest request)
        {
            string method = nameof(EditDeposit);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var entity = request.ToEntity(true);

                var existingEntity = await DbContext.GetDeposits(null, null, null, request.id).Include(x => x.purchases).FirstAsync();
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }

                existingEntity.account_id = entity.account_id;
                existingEntity.status = entity.status;
                existingEntity.unit_cost_id = entity.unit_cost_id;
                existingEntity.status = entity.status;
                existingEntity.count = entity.count;
                existingEntity.amount = entity.amount;


                var purchases_request = request.ToPurchasesRequest(true);
                if (purchases_request.Sum(x => x.amount) != existingEntity.amount)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.BadRequest;
                    response.ErrorMessage = Constants.MessageText.PurcheseAmountSumError;
                    return response.ToHttpResponse(Logger, method);
                }
                LogHandler.LogMethod(LogHandler.EventType.Operation, Logger, method, purchases_request);
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
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                var entity_list = new List<Deposit> { entity }
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
        public async Task<IActionResult> DeleteDeposit(long id)
        {
            string method = nameof(DeleteDeposit);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetDeposit(new Deposit { id = id });
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
        public async Task<IActionResult> GetDeposit(long id)
        {
            string method = nameof(GetDeposit);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingList = await DbContext.GetDeposits(null, null, null, id).Include(x => x.unit_cost).ToListAsync();
                if (!existingList.Any())
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
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
                response.ErrorCode = (int)ErrorHandler.ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger, method);
        }
    }

    public class DefaultController : ControllerBase
    {
        protected readonly IDistributedCache _distributedCache;
        protected readonly ILogger Logger;
        protected readonly HSBorsDb DbContext;

        public DefaultController(IDistributedCache distributedCache, ILogger<DefaultController> logger, HSBorsDb dbContext)
        {
            _distributedCache = distributedCache;
            Logger = logger;
            DbContext = dbContext;
        }
    }

#pragma warning restore CS1591
}