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
    public class SettingController : DefaultController
    {
        public SettingController(IDistributedCache distributedCache, ILogger<SettingController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSetting(long id)
        {
            string method = nameof(GetSetting);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetSetting(new Setting { id = id });
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

        [HttpPost("search")] 
        public async Task<IActionResult> SearchSetting()
        {
            string method = nameof(SearchSetting);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            { 
                var query = await DbContext.GetSettings()
                    //  .Include(x=>x.fund)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.type,
                        x.key,
                        x.value
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
        public async Task<IActionResult> AddSetting([FromBody] AddSettingRequest request)
        {
            string method = nameof(AddSetting);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var user = request.ToEntity();

                if (user.type == SettingType.AnnualProfit)
                {
                    var existingEntity = DbContext.GetSettings(null, user.type);
                    if (existingEntity?.Count()>0)
                    {
                        response.ErrorCode = (int)ErrorHandler.ErrorCode.AddRepeatedEntity;
                        return response.ToHttpResponse(Logger, method);
                    }
                }

                DbContext.Add(user);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                var entity_list = new List<Setting> { user }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.type,
                        x.key,
                        x.value,
                        x.status
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
        public async Task<IActionResult> EditSetting([FromBody] AddSettingRequest request)
        {
            string method = nameof(EditSetting);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger, method);

                var entity = request.ToEntity(true); 

                var existingEntity = await DbContext.GetSetting(entity);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger, method);
                }

                existingEntity.type = entity.type;
                existingEntity.key = entity.key;
                existingEntity.value = entity.value;
                existingEntity.status = entity.status; 

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorHandler.ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger, method);
                }
                var entity_list = new List<Setting> { entity }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.type,
                        x.key,
                        x.value,
                        x.status
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
        public async Task<IActionResult> DeleteSetting(long id)
        {
            string method = nameof(DeleteSetting);
            LogHandler.LogMethod(LogHandler.EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetSetting(new Setting { id = id });
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

    }
}