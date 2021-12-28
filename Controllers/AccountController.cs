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
    public class AccountController : DefaultController
    {
        public AccountController(IDistributedCache distributedCache, ILogger<AccountController> logger, HSBorsDb dbContext) :
            base(distributedCache, logger, dbContext)
        {
        }
        
        [HttpPost("search")]
        [DisplayName("جستجوی حساب")]
        public async Task<IActionResult> SearchAccount([FromBody] SearchAccountRequest request)
        {
            string method = nameof(SearchAccount);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            PagedResponse<object> response = new PagedResponse<object>();

            try
            {
                var query = await DbContext.GetAccounts(request).Paging(response, request.page_start, request.page_size)
                    .Include(x => x.fund).Include(x => x.accounter)
                    .ToListAsync();
                var entity_list = query
                    .Select(x => new
                    {
                        x.id,
                        x.name,
                        x.fund_id,
                        x.fund_name,
                        x.accounter_id,
                        x.accounter_name
                    });
                response.Model = entity_list;
                response.ErrorCode = (int?)ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(Logger, response, method, ex);
            }
            return response.ToHttpResponse(Logger,Request.HttpContext);

        }

        [DisplayName("افزودن حساب")]
        [HttpPost("add")]
        public async Task<IActionResult> AddAccount([FromBody] AddAccountRequest request)
        {
            string method = nameof(AddAccount);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var account = request.ToEntity();

                var existingEntity = await DbContext.GetAccount(account);
                if (existingEntity != null)
                {
                    response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                DbContext.Add(account);
                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<Account> { account }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.accounter_id,
                        x.accounter_name,
                        x.fund_name,
                        x.fund_id,
                        x.name,
                        x.status,
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

        [DisplayName("ویرایش حساب")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditAccount([FromBody] AddAccountRequest request)
        {
            string method = nameof(EditAccount);
            LogHandler.LogMethod(EventType.Call, Logger, method, request);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                if (!request.CheckValidation(response))
                    return response.ToHttpResponse(Logger,Request.HttpContext);

                var account = request.ToEntity();
                account.id = request.id;

                var existingEntity = await DbContext.GetAccount(account);
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }

                existingEntity.accounter_id = account.accounter_id;
                existingEntity.fund_id = account.fund_id;
                existingEntity.name = account.name;
                existingEntity.no = account.no;
                existingEntity.status = account.status;

                int save = await DbContext.SaveChangesAsync();
                if (save == 0)
                {
                    response.ErrorCode = (int)ErrorCode.DbSaveNotDone;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                var entity_list = new List<Account> { account }
                    .Select(x => new
                    {
                        x.id,
                        x.create_date,
                        x.creator_id,
                        x.accounter_id,
                        x.accounter_name,
                        x.fund_name,
                        x.fund_id,
                        x.name,
                        x.status,
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
        [DisplayName("حذف حساب")]
        public async Task<IActionResult> DeleteAccount(long id)
        {
            string method = nameof(DeleteAccount);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetAccount(new Account { id = id });
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

        [HttpGet("{id}")]
        [System.ComponentModel.DisplayName("مشاهده حساب")]
        public async Task<IActionResult> GetAccount(long id)
        {
            string method = nameof(GetAccount);
            LogHandler.LogMethod(EventType.Call, Logger, method, id);
            SingleResponse<object> response = new SingleResponse<object>();

            try
            {
                var existingEntity = await DbContext.GetAccount(new Account { id = id });
                if (existingEntity == null)
                {
                    response.ErrorCode = (int)ErrorCode.NoContent;
                    return response.ToHttpResponse(Logger,Request.HttpContext);
                }
                response.Model = new List<Account> { existingEntity }.Select(x => new
                {
                    x.accounter_id,
                    x.accounter_name,
                    x.create_date,
                    x.creator_id,
                    x.fund_id,
                    x.fund_name,
                    x.id,
                    x.name,
                    x.no
                }).First(); ;
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