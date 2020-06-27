using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HSBors.Models
{
#pragma warning disable CS1591

    public static class HSBorsDbExtensions
    {
        public static IQueryable<Deposit> GetDeposits(this HSBorsDb db, long? unit_cost_id = null, long? account_id = null, EntityStatus? status = null, long? id = null, long? fund_id = null)
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
            return query.OrderByDescending(x=>x.date);
        }
        public static IQueryable<Deposit> GetDeposits(this HSBorsDb db, SearchDepositRequest request)
        {
            return GetDeposits(db, request.unit_cost_id, request.account_id, (EntityStatus?)request.status, null, request.fund_id);

        }
        public static async Task<Deposit> GetDeposit(this HSBorsDb db, Deposit entity)
       => await db.Deposits.FirstOrDefaultAsync(item => item.id == entity.id);
        public static IQueryable<Fund> GetFunds(this HSBorsDb db, long? id = null, long? account_id = null, EntityStatus? status = null)
        {
            var query = db.Funds.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == status);

            return query;
        }
        public static IQueryable<Fund> GetFunds(this HSBorsDb db, SearchFundRequest request)
        {
            return GetFunds(db, request.id, request.account_id, (EntityStatus?)request.status);

        }
        public static async Task<Fund> GetFund(this HSBorsDb db, Fund entity)
           => await db.Funds.FirstOrDefaultAsync(item => (item.name == entity.name) || item.id == entity.id);
        public static IQueryable<UnitCost> GetUnitcosts(this HSBorsDb db, long? id = null, long? fund_id = null)
        {
            var query = db.UnitCosts.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (fund_id.HasValue)
                query = query.Where(item => item.fund_id == fund_id);

            return query.OrderByDescending(x => x.date);
        }
        public static IQueryable<UnitCost> GetUnitcosts(this HSBorsDb db, SearchUnitRequest request)
        {
            return GetUnitcosts(db, request.id, request.fund_id);

        }
        public static async Task<UnitCost> GetUnitcost(this HSBorsDb db, UnitCost entity)
         => await db.UnitCosts.FirstOrDefaultAsync(item => (item.date == entity.date && item.fund_id == entity.fund_id) || item.id == entity.id);
        public static IQueryable<Account> GetAccounts(this HSBorsDb db, long? id = null, long? fund_id = null, EntityStatus? status = null)
        {
            var query = db.Accounts.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == status);
            if (fund_id.HasValue)
                query = query.Where(item => item.fund_id == fund_id);

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
        => await db.Accounts.FirstOrDefaultAsync(item => item.id == entity.id || (item.fund_id == entity.fund_id && item.name == entity.name));
        public static IQueryable<User> GetUsers(this HSBorsDb db, long? id = null, int? status = null)
        {
            var query = db.Users.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (status.HasValue)
                query = query.Where(item => item.status == (EntityStatus)status);

            return query;
        }
        public static IQueryable<User> GetUsers(this HSBorsDb db, SearchUserRequest request)
        {
            return GetUsers(db, request.id, request.status);

        }
        public static async Task<User> GetUser(this HSBorsDb db, User entity)
        => await db.Users.FirstOrDefaultAsync(item => item.id == entity.id);
        public static IQueryable<Purchase> GetPurchases(this HSBorsDb db, long? id = null, long? deposit_id = null, int? status = null)
        {
            var query = db.Purchases.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (deposit_id.HasValue)
                query = query.Where(item => item.deposit_id == deposit_id);

            return query;
        }
        public static IQueryable<Purchase> GetPurchases(this HSBorsDb db, SearchPurchaseRequest request)
        {
            return GetPurchases(db, request.id, request.deposit_id, request.status);

        }
        public async static Task<IEnumerable<object>> GetPurchaseList(this HSBorsDb db, PagedResponse<object> response, SearchPurchaseRequest request)
        {
            var query = await GetPurchases(db, request).Paging(response, request.page_start, request.page_size)
                  .Include(x => x.bank_copartner).Include(x => x.buyer)
                    .ToListAsync();
            var entity_list = query
                .Select(x => new
                {
                    x.id,
                    x.amount,
                    x.bank_copartner_id,
                    x.bank_copartner_name,
                    x.bank_copartner_intrest,
                    x.bank_copartner_percent,
                    x.buyer_id,
                    x.buyer_name
                });
            return entity_list;
        }
        public static async Task<Purchase> GetPurchase(this HSBorsDb db, Purchase entity)
        => await db.Purchases.FirstOrDefaultAsync(item => item.id == entity.id);

        public static IQueryable<Setting> GetSettings(this HSBorsDb db, long? id = null, SettingType? type = null)
        {
            var query = db.Settings.AsQueryable();

            if (id.HasValue)
                query = query.Where(item => item.id == id);
            if (type.HasValue)
                query = query.Where(item => item.type == type);

            return query;
        } 
        public static async Task<Setting> GetSetting(this HSBorsDb db, Setting entity)
        => await db.Settings.FirstOrDefaultAsync(item => item.id == entity.id);
    }

    public static class DepositExtensions
    {
        public static IQueryable<Deposit> Test(this IQueryable<Deposit> deposits)
        {
            var query = deposits;
            return query;
        }
    }


    public static class PurchaseExtensions
    {
        public static void UpdateProperty(this Purchase entity, Purchase new_entity)
        {
            entity.amount = new_entity.amount;
            entity.bank_copartner_id = new_entity.bank_copartner_id;
            entity.bank_copartner_intrest = new_entity.bank_copartner_intrest;
            entity.bank_copartner_percent = new_entity.bank_copartner_percent;
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
                response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                return false;
            }

            existingEntity.UpdateProperty(request.ToEntity());

            return true;
        }
        public async static Task<bool> Edit(this Purchase entity, HSBorsDb db, SingleResponse<object> response)
        {
            var existingEntity = await db.GetPurchase(entity);
            if (existingEntity == null)
            {
                response.ErrorCode = (int)ErrorHandler.ErrorCode.NoContent;
                return false;
            }

            existingEntity.UpdateProperty(entity);

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
            return pageSize > 0 && pageStart > 0 ? query.Skip((pageStart - 1) * pageSize).Take(pageSize) : query;

        }
    }


#pragma warning restore CS1591
}