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
    public class UnitCostController : DefaultController
    {
        public UnitCostController(IDistributedCache distributedCache, ILogger<UnitCostController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [HttpPost("search")]
        [DisplayName("جستجوی واحد")]
        public async Task<IActionResult> SearchUnitcost([FromBody] SearchUnitRequest request)
        {
            string method = nameof(SearchUnitcost);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {

                var query = await DbContext.GetUnitcosts(request).Paging(response, request.page_start, request.page_size)
                    .Include(x => x.fund)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.cancel_cost, 
                        x.pdate,
                        x.fund_id,
                        x.fund_name,
                        x.issue_cost,
                        x.update_type
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
        [DisplayName("افزودن واحد")]
        public async Task<IActionResult> AddUnitCost([FromBody] AddUnitCostRequest request)
        {
            string method = nameof(AddUnitCost);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var unit = await request.ConvertForAdd(DbContext, response,UpdateType.add);
                if (unit == null) return response.ToHttpResponse(Logger,Request.HttpContext);

                DbContext.Add(unit);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<UnitCost> { unit }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id, 
                        x.pdate,
                        x.cancel_cost,
                        x.issue_cost,
                        x.fund_id
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

        [DisplayName("افزودن قطعی واحد")]
        [HttpPost("add/override")]
        public async Task<IActionResult> AddUnitCostOverride([FromBody] AddUnitCostRequest request)
        {
            string method = nameof(AddUnitCostOverride);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var unit = await request.ConvertForAdd(DbContext, response,UpdateType.add_override);
                if (unit != null) DbContext.Add(unit);
                else
                {
                    unit = await request.ConvertForEdit(DbContext, response,UpdateType.add_override);
                    if (unit == null) return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                 
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<UnitCost> { unit }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id, 
                        x.pdate,
                        x.cancel_cost,
                        x.issue_cost,
                        x.fund_id
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

        [HttpPut("edit")]
        [DisplayName("ویرایش واحد")]
        public async Task<IActionResult> EditUnitCost([FromBody] AddUnitCostRequest request)
        {
            string method = nameof(EditUnitCost);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = await request.ConvertForEdit(DbContext, response,UpdateType.edit);
                if (entity == null) return response.ToHttpResponse(Logger,Request.HttpContext);

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<UnitCost> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.pdate,
                        x.cancel_cost,
                        x.issue_cost,
                        x.fund_id
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

        [DisplayName("حذف واحد")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUnitCost(long id)
        {
            string method = nameof(DeleteUnitCost);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetUnitcost(new UnitCost { id = id });
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

        [DisplayName("مشاهده واحد")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnitCost(long id)
        {
            string method = nameof(GetUnitCost);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetUnitcost(new UnitCost { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
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
}