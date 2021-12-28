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
    public class ConventionController : DefaultController
    {
        public ConventionController(IDistributedCache distributedCache, ILogger<ConventionController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [DisplayName("جستجوی قرارداد")]
        [HttpPost("search")] 
        public async Task<IActionResult> SearchConvention([FromBody] SearchConventionRequest request)
        {
            string method = nameof(SearchConvention);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {

                var query = await DbContext.GetConventions(request).Paging(response, request.page_start, request.page_size) 
                    .Include(x=>x.first_user).Include(x=>x.second_user)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.first_user_id,
                        x.second_user_id,
                        first_user_full_name=x.first_user?.full_name,
                        second_user_full_name = x.second_user?.full_name,
                        x.bank_copartner_intrest,
                        x.bank_copartner_percent
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

        [DisplayName("افزودن قرارداد")]
        [HttpPost("add")] 
        public async Task<IActionResult> AddConvention([FromBody] AddConventionRequest request)
        {
            string method = nameof(AddConvention);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = request.ToEntity();

                var existingEntity = await DbContext.GetConvention(entity);
                if (existingEntity != null)
                {
                    response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                DbContext.Add(entity);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<Convention> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.first_user_id,
                        x.second_user_id,
                        x.bank_copartner_intrest,
                        x.bank_copartner_percent
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

        [DisplayName("حذف قرارداد")]
        [HttpDelete("delete/{id}")] 
        public async Task<IActionResult> DeleteConvention(long id)
        {
            string method = nameof(DeleteConvention);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetConvention(new Convention { id = id });
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

        [DisplayName("ویرایش قرارداد")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditConvention([FromBody] AddConventionRequest request)
        {
            string method = nameof(EditConvention);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var entity = request.ToEntity();
                entity.id = request.id;

                var existingEntity = await DbContext.GetConvention(entity);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                existingEntity.first_user_id = entity.first_user_id;
                existingEntity.second_user_id = entity.second_user_id;
                existingEntity.bank_copartner_intrest = entity.bank_copartner_intrest;
                existingEntity.bank_copartner_percent = entity.bank_copartner_percent; 

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<Convention> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id, 
                        x.second_user_id,
                        x.bank_copartner_intrest,
                        x.bank_copartner_percent
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

        [DisplayName("مشاهده قرارداد")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConvention(long id)
        {
            string method = nameof(GetConvention);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetConvention(new Convention { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                response.Model =new List<Convention> { existingEntity }.Select(x=>new { 
                x.bank_copartner_intrest,
                x.bank_copartner_percent,
                x.create_date,
                x.creator_id,
                x.first_user_id,
                x.id,
                x.second_user_id,
                x.update_type
                }).First();
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