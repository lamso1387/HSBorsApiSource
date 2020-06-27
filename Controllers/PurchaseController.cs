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
    public class PurchaseController : DefaultController
    {
        public PurchaseController(IDistributedCache distributedCache, ILogger<PurchaseController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchPurchase([FromBody] SearchPurchaseRequest request)
        {
            string method = nameof(SearchPurchase);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var entity_list = await DbContext.GetPurchaseList(response,request);
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
        public async Task<IActionResult> GetPurchse(long id)
        {
            string method = nameof(GetPurchse);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetPurchase(new Purchase { id = id });
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

        [HttpPut("edit")]
        public async Task<IActionResult> EditPurchase([FromBody] AddPurchaseRequest request)
        {
            string method = nameof(EditPurchase);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                Purchase entity = new Purchase();
                bool editOK = await entity.Edit(DbContext, request, response);
                if (!editOK) return response.ToHttpResponse(Logger, method);

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
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