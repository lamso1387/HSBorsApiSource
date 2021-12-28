using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography.X509Certificates;
using HSBors.Middleware;
using HSBors.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Logging;

namespace HSBors.Models
{
#pragma warning disable CS1591

    public static class ResponseExtension
    {
        public static IActionResult ToHttpResponse(this IResponse response, ILogger Logger, HttpContext context)
        { 
            var error = ErrorProp.GetError((ErrorCode)response.ErrorCode, response.ErrorMessage); 
            response.ErrorMessage = error.message;  
            return CreateHttpObject(response, error.status, Logger, context.GetActionName());

        }
        public static IActionResult ToPagedResponse<T>(this IPagedResponse<T> response, ILogger Logger, HttpContext context,IEnumerable<T> model)
        {
            response.Model = model;
            ErrorCode error_code = ErrorCode.OK;
            response.ErrorCode = (int)error_code; 
            var error = ErrorProp.GetError(error_code); 
            response.ErrorMessage = error.message;
            return CreateHttpObject(response, error.status, Logger,context.GetActionName());
        }

        
        private static IActionResult CreateHttpObject(IResponse response, HttpStatusCode status, ILogger Logger, string method)
        {
            ObjectResult result = new ObjectResult(response);
            result.StatusCode = (int)status;
            LogHandler.LogMethod(EventType.Return, Logger, method, result);
            return result;
        }


    }

    public static class HttpContextExtentions
    {
        public static string GetActionName(this HttpContext context )
        {
            return context.GetRouteData().Values["action"].ToString();
        }
        public static bool NeedAuth(this HttpContext context, ref string action)
        {
            action = context.GetActionName();
            return !Constants.Actions.NoAuth.Contains(action);
        }
    }
    public static class HSBorsDbExtensions
    {
        public static IQueryable<Deposit> GetDeposits(this HSBorsDb db, HSViewMode view_mode, long? unit_cost_id = null, long? account_id = null, EntityStatus? status = null, long? id = null, long? fund_id = null)
        {
            var query = db.Deposits.AsQueryable();

            if (unit_cost_id.HasValue)
                query = query.Where(item => item.unit_cost_id == unit_cost_id);
            if (account_id.HasValue)
                query = query.Where(item => item.account_id == account_id);
            if (status.HasValue)
                query = query.Where(item => item.status == status);
            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (fund_id.HasValue)
                query = query.Where(item => item.account.fund_id == fund_id);

            Func<IQueryable<Deposit>, IQueryable<Deposit>> my_func = null;
            switch (view_mode)
            {
                case HSViewMode.ReadOnly:
                    my_func = DepositExtensions.GetMyViewData;
                    break;
                case HSViewMode.Edit:
                    my_func = null;// DepositExtensions.GetMyData;
                    break;
            }
            query = query.FilterNonActionAccess(null, my_func);

            return query;//.OrderByDescending(x => x.date); causes error because include unitcost not done before
        }

        public static IQueryable<Deposit> GetDeposits(this HSBorsDb db, HSViewMode view_mode, SearchDepositRequest request)
        {
            return GetDeposits(db, view_mode, request.unit_cost_id, request.account_id, (EntityStatus?)request.status, null, request.fund_id);

        }
        public static async Task<Deposit> GetDeposit(this HSBorsDb db, Deposit entity)
        => await db.Deposits.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id);

        public static IQueryable<Fund> GetFunds(this HSBorsDb db, long? id = null, long? account_id = null, EntityStatus? status = null)
        {
            var query = db.Funds.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == status);
            query = query.FilterNonActionAccess(null, null);

            return query;
        }
        public static IQueryable<Fund> GetFunds(this HSBorsDb db, SearchFundRequest request)
        {
            return GetFunds(db, request.id, request.account_id, (EntityStatus?)request.status);

        }
        public static async Task<Fund> GetFund(this HSBorsDb db, Fund entity)
           => await db.Funds.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => (item.name == entity.name) || item.id == entity.id);
        public static IQueryable<UnitCost> GetUnitcosts(this HSBorsDb db, long? id = null, long? fund_id = null)
        {
            var query = db.UnitCosts.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (fund_id.HasValue)
                query = query.Where(item => item.fund_id == fund_id);
            query = query.FilterNonActionAccess(null, null);
            return query.OrderByDescending(x => x.pdate);
        }
        public static IQueryable<UnitCost> GetUnitcosts(this HSBorsDb db, SearchUnitRequest request)
        {
            return GetUnitcosts(db, request.id, request.fund_id);

        }
        public static async Task<UnitCost> GetUnitcost(this HSBorsDb db, UnitCost entity)
         => await db.UnitCosts.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => (item.pdate == entity.pdate && item.fund_id == entity.fund_id) || item.id == entity.id);
        public static IQueryable<Account> GetAccounts(this HSBorsDb db, long? id = null, long? fund_id = null, EntityStatus? status = null)
        {
            var query = db.Accounts.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == status);
            if (fund_id.HasValue)
                query = query.Where(item => item.fund_id == fund_id);

            query = query.FilterNonActionAccess(null, null);// nameof(Account.accounter_id), null);

            return query;
        }
        public static IQueryable<Account> GetAccounts(this HSBorsDb db, SearchAccountRequest request)
        {
            EntityStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.status))
                status = EnumConvert.StringToEnum<EntityStatus>(request.status);
            return GetAccounts(db, request.id, request.fund_id, status);

        }
        public static async Task<Account> GetAccount(this HSBorsDb db, Account entity)
        => await db.Accounts.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id || (item.fund_id == entity.fund_id && item.name == entity.name));
        public static IQueryable<User> GetUsers(this HSBorsDb db, long? id = null, int? status = null)
        {
            var query = db.Users.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == (EntityStatus)status);

            query = query.FilterNonActionAccess(nameof(User.id), null);

            return query;
        }
        public static IQueryable<User> GetUsers(this HSBorsDb db, SearchUserRequest request)
        {
            return GetUsers(db, request.id, request.status);

        }
        public static IQueryable<Gainer> GetGainers(this HSBorsDb db, long? id = null)
        {
            var query = db.Gainers.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);

            query = query.FilterNonActionAccess(nameof(Gainer.primary_user_id), null);

            return query;
        }
        public static IQueryable<Gainer> GetGainers(this HSBorsDb db, SearchGainerRequest request)
        {
            return GetGainers(db, request.id);

        }
        public static async Task<User> GetUser(this HSBorsDb db, User entity)
        => await db.Users.FilterNonActionAccess(nameof(User.id), null).FirstOrDefaultAsync(item => item.id == entity.id || (item.national_code == entity.national_code));
        public static async Task<Gainer> GetGainer(this HSBorsDb db, Gainer entity, bool filter_access)
        {
            IQueryable<Gainer> gainers = db.Gainers;
            if (filter_access)
                gainers = gainers.FilterNonActionAccess(nameof(Gainer.primary_user_id), null);
            return await gainers.FirstOrDefaultAsync(item => item.id == entity.id || (item.primary_user_id == entity.primary_user_id && item.secondary_user_id == entity.secondary_user_id));
        }
        public static IQueryable<Purchase> GetPurchases(this HSBorsDb db, long? id = null, long? deposit_id = null, int? status = null, long? buyer_id = null)
        {
            var query = db.Purchases.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (deposit_id.HasValue)
                query = query.Where(item => item.deposit_id == deposit_id);
            if (buyer_id.HasValue)
                query = query.Where(item => item.buyer_id == buyer_id);

            query = query.FilterNonActionAccess(nameof(Purchase.buyer_id), null);

            return query;
        }
        public static IQueryable<Purchase> GetPurchases(this HSBorsDb db, SearchPurchaseRequest request)
        {
            return GetPurchases(db, request.id, request.deposit_id, request.status, request.buyer_id);

        }
        public async static Task<IEnumerable<object>> GetPurchaseList(this HSBorsDb db, PagedResponse<object> response, SearchPurchaseRequest request)
        {
            var query = await GetPurchases(db, request).Paging(response, request.page_start, request.page_size)
                  .Include(x => x.convention).ThenInclude(c => c.second_user)
                  .Include(x => x.buyer)
                    .ToListAsync();
            var entity_list = query
                .Select(x => new
                {
                    x.id,
                    x.amount,
                    bank_copartner_id = x.convention?.second_user?.id,
                    x.bank_copartner_name,
                    x.convention?.bank_copartner_intrest,
                    x.convention?.bank_copartner_percent,
                    x.buyer_id,
                    x.buyer_name
                });
            return entity_list;
        }
        public static async Task<Purchase> GetPurchase(this HSBorsDb db, Purchase entity)
        => await db.Purchases.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id);
        public static IQueryable<Setting> GetSettings(this HSBorsDb db, long? id = null, SettingType? type = null)
        {
            var query = db.Settings.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (type.HasValue)
                query = query.Where(item => item.type == type);
            query = query.FilterNonActionAccess(null, null);
            return query;
        }
        public static async Task<Setting> GetSetting(this HSBorsDb db, Setting entity)
        => await db.Settings.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id);
        public static async Task<Convention> GetConvention(this HSBorsDb db, Convention entity)
       => await db.Conventions.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id
        || (item.first_user_id == entity.first_user_id && item.second_user_id == entity.second_user_id));
        public static IQueryable<Convention> GetConventions(this HSBorsDb db, long? id = null, long? first_user_id = null, long? second_user_id = null)
        {
            var query = db.Conventions.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (first_user_id.HasValue)
                query = query.Where(item => item.first_user_id == first_user_id);
            if (second_user_id.HasValue)
                query = query.Where(item => item.second_user_id == second_user_id);

            query = query.FilterNonActionAccess(null, null);// nameof(Convention.first_user_id), null);

            return query;
        }
        public static IQueryable<Convention> GetConventions(this HSBorsDb db, SearchConventionRequest request)
        {
            return GetConventions(db, request.id, request.first_user_id, request.second_user_id);

        }
        public static async Task<Role> GetRole(this HSBorsDb db, Role entity)
    => await db.Roles.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id || item.name == entity.name);
        public static IQueryable<Role> GetRoles(this HSBorsDb db, long? id = null, string name = null)
        {
            var query = db.Roles.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(item => item.name == name);
            query = query.FilterNonActionAccess(null, null);
            return query;
        }
        public static async Task<UserRole> GetUserRole(this HSBorsDb db, UserRole entity)
=> await db.UserRoles.FilterNonActionAccess(null, null).FirstOrDefaultAsync(item => item.id == entity.id || (item.user_id == entity.user_id && item.role_id == entity.role_id));

        public static IQueryable<UserRole> GetUserRoles(this HSBorsDb db, long role_id)
=> db.UserRoles.FilterNonActionAccess(null, null).Where(item => item.role_id == role_id).AsQueryable();

    }
    public static class FundExtensions
    {
        public static bool DeleteSingleFund(this Fund fund, HSBorsDb db, ref SingleResponse<object> response)
        {
            if (fund == null)
            {
                response.ErrorCode = (int)ErrorCode.NoContent;
                return false;
            }

            db.Remove(fund);

            return true;
        }

    }
    public static class DepositExtensions
    {
        public static IQueryable<Deposit> Test(this IQueryable<Deposit> deposits)
        {
            var query = deposits;
            return query;
        }
        public static bool PurchasesAmountsOK(this Deposit deposit, SingleResponse<object> response)
        {
            if (deposit.purchases.Sum(x => x.amount) != deposit.amount)
            {
                response.ErrorCode = (int)ErrorCode.BadRequest;
                response.ErrorMessage = Constants.MessageText.PurcheseAmountSumError;
                return false;
            }
            return true;
        }
        public static IQueryable<Deposit> GetMyData(this IQueryable<Deposit> query)
        {
            query = query.Include(x => x.account).Where(x => x.account.accounter_id == UserSession.Id);
            return query;
        }
        public static IQueryable<Deposit> GetMyViewData(this IQueryable<Deposit> query)
        {
            query = query.Include(x => x.purchases).Include(x => x.account)
                .Where(x => x.purchases.Any(y => y.buyer_id == UserSession.Id) || x.account.accounter_id == UserSession.Id);
            return query;
        }

    }

    public static class UnitCostExtensions
    {
        public async static Task<UnitCost> ConvertForAdd(this AddUnitCostRequest request, HSBorsDb db, SingleResponse<object> response, UpdateType update_type)
        {
            var unit = request.ToEntity(update_type);

            var existingEntity = await db.GetUnitcost(unit);
            if (existingEntity != null)
            {
                response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                return null;
            }
            return unit;
        }
        public async static Task<UnitCost> ConvertForEdit(this AddUnitCostRequest request, HSBorsDb db, SingleResponse<object> response, UpdateType update_type)
        {
            var entity = request.ToEntity(update_type);
            entity.id = request.id;

            var existingEntity = await db.GetUnitcost(entity);
            if (existingEntity == null)
            {
                response.ErrorCode = (int)ErrorCode.NoContent;
                return null;
            }

            existingEntity.cancel_cost = entity.cancel_cost;
            existingEntity.pdate = entity.pdate;
            existingEntity.fund_id = entity.fund_id;
            existingEntity.issue_cost = entity.issue_cost;
            return entity;
        }
        //public static IQueryable<UnitCost> UnionMyData(this IQueryable<UnitCost> query)
        //{
        //    query = query.Include(x => x.deposits).ThenInclude(x => x.purchases)
        //           .Where(x => x.deposits.Any(y => y.purchases.Any(z => z.buyer_id == UserSession.Id)));
        //    return query;
        //}
    }
    public static class UserExtensions
    {
        public static User UpdatePasswordHash(this User user, HSBorsDb db)
        {
            if (!string.IsNullOrWhiteSpace(user.password))
            {
                new UserService(db).CreatePasswordHash(user.password, out byte[] passwordHash, out byte[] passwordSalt);
                user.password_hash = passwordHash;
                user.password_salt = passwordSalt;
            }
            return user;
        }
    }

    public static class PurchaseExtensions
    {
        public static bool IsConventionsUnique(this List<Purchase> list, HSBorsDb db, SingleResponse<object> response)
        {
            foreach (var item in list)
            {
                if (item.convention != null)
                {
                    var existingConvention = db.GetConvention(item.convention).Result;
                    if (existingConvention != null)
                    {
                        response.ErrorCode = (int)ErrorCode.AddRepeatedEntity;
                        return false;
                    }
                }
            }
            return true;
        }
        public static void UpdateProperty(this Purchase entity, Purchase new_entity)
        {
            entity.amount = new_entity.amount;
            entity.buyer_id = new_entity.buyer_id;
            entity.deposit_id = new_entity.deposit_id;
        }
        public async static Task<bool> Edit(this Purchase purchase, HSBorsDb db, AddPurchaseRequest request, SingleResponse<object> response)
        {
            if (!request.CheckValidation(response)) return false;
            var entity = request.ToEntity(true);
            var existingEntity = await db.GetPurchase(entity);
            if (existingEntity == null)
            {
                response.ErrorCode = (int)ErrorCode.NoContent;
                return false;
            }

            existingEntity.UpdateProperty(request.ToEntity());

            return true;
        }
    }

    public static class IQueryableExtensions
    {
        public static IQueryable<TModel> Paging<TModel>(this IQueryable<TModel> query, PagedResponse<object> response, int pageStart = 0, int pageSize = 0) where TModel : class
        {
            response.ItemsCount = query.Count();
            response.PageNumber = pageStart;
            response.PageSize = pageSize;
            return pageSize > 0 && pageStart > 0 ? query.Skip((pageStart - 1) * pageSize).Take(pageSize) :
                query.Skip(0);

        }
        public static IQueryable<T> FilterNonActionAccess<T>(this IQueryable<T> query, string my_id, Func<IQueryable<T>, IQueryable<T>> MyUnionFunc)
        {
            bool all_data = UserSession.GetNonActionAccess(NonActionAccess.AllData);
            bool my_data = UserSession.GetNonActionAccess(NonActionAccess.MyData);
            bool share_data = UserSession.GetNonActionAccess(NonActionAccess.MyShareData);
            IQueryable<T> data_to_union = null;
            List<string> where_list = new List<string>();
            string share_id = nameof(User.creator_id);
            where_list.Add($"{all_data}");

            if (!string.IsNullOrWhiteSpace(my_id))
                where_list.Add($"({my_data} and {my_id}=={UserSession.Id})");
            else if (my_data && MyUnionFunc != null) data_to_union = MyUnionFunc(query);

            where_list.Add($"({share_data} and { share_id}=={ UserSession.Id})");
            string where_clause = string.Join(" || ", where_list);
            query = query.Where(where_clause).AsQueryable();
            if (data_to_union != null) query = query.Union(data_to_union).OrderBy(nameof(CommonProperty.create_date));
            return query;
        }
    }

    public static class StringDateExtensions
    {
        public static DateTime ToEnDate(this string pdate)
        {
            return HSDate.ToEnglish(pdate);

        }
        public static string ToPDate(this DateTime? date)
        {
            if (date == null) return null;
            return HSDate.ToHSDate(date);

        }
    }


#pragma warning restore CS1591
}