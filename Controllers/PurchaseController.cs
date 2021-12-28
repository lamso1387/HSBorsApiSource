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
using System.ComponentModel;

namespace HSBors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : DefaultController
    {
        public PurchaseController(IDistributedCache distributedCache, ILogger<PurchaseController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [DisplayName("جستجوی خرید")]
        [HttpPost("search")]
        public async Task<IActionResult> SearchPurchase([FromBody] SearchPurchaseRequest request)
        {
            string method = nameof(SearchPurchase);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var entity_list = await DbContext.GetPurchaseList(response, request);
                response.Model = entity_list;
                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }

            return response.ToHttpResponse(Logger,Request.HttpContext);

        }

        [DisplayName("مشاهده خرید")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchse(long id)
        {
            string method = nameof(GetPurchse);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetPurchases(id).Include(c => c.convention).ToListAsync();

                if (existingEntity == null || existingEntity.Count == 0)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                response.Model = existingEntity.Select(x => new
                {
                    x.amount,
                    x.bank_copartner_name,
                    x.buyer_id,
                    x.buyer_name,
                    x.convention_id,
                    x.create_date,
                    x.deposit_id,
                    x.id,
                    x.convention?.first_user_id,
                    x.convention?.second_user_id,
                    x.convention?.bank_copartner_intrest,
                    x.convention?.bank_copartner_percent,
                    }).First();

                response.ErrorCode = (int)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);
        }

        [DisplayName("ویرایش خرید")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditPurchase([FromBody] AddPurchaseRequest request)
        {
            string method = nameof(EditPurchase);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                Purchase entity = new Purchase();
                bool editOK = await entity.Edit(DbContext, request, response);
                if (!editOK) return response.ToHttpResponse(Logger,Request.HttpContext);

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<Purchase> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.deposit_id,
                        x.buyer_id
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

    }
}