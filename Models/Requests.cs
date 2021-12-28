using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using HSBors.Middleware;
using HSBors.Services;

namespace HSBors.Models
{
#pragma warning disable CS1591
    public class DateRangeAttribute : RangeAttribute
    {
        public DateRangeAttribute()
           : base(typeof(DateTime), DateTime.Now.AddYears(-20).ToShortDateString(), DateTime.Now.AddYears(20).ToShortDateString()) { }
    }
    public class HsPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            string pass = value.ToString();
            if (pass.Length < 8) return false;
            return true;
        }
    }
    public abstract class BorsRequest
    {
        public long id { get; set; }
        public bool CheckValidation(IResponse response)
        {
            if (CheckAttrbuteValidation())
                if (CheckPropertyValidation())
                    if (!UserSession.GetNonActionAccess(NonActionAccess.AllData))
                        CheckAccessValidation();
            if (validation_errors.Any())
            {
                response.ErrorCode = (int)ErrorCode.BadRequest;
                response.ErrorMessage = validation_errors.First();
                return false;
            }
            return true;
        }
        public void CheckValidation2(IResponse response)
        {
            if (CheckAttrbuteValidation())
                if (CheckPropertyValidation())
                    if (!UserSession.GetNonActionAccess(NonActionAccess.AllData))
                        CheckAccessValidation();
            if (validation_errors.Any())
                throw new AppException(validation_errors.First());

        }
        public bool CheckAttrbuteValidation()
        {
            validation_errors = SRL.ClassManagement.CheckValidationAttribute(this);
            return validation_errors.Count == 0 ? true : false;
        }
        protected List<string> validation_errors { get; set; }
        protected virtual bool CheckPropertyValidation() { return true; }
        protected virtual bool CheckAccessValidation() { return true; }
    }
    public class PagedRequest
    {
        /// <summary>
        /// 1
        /// </summary>
        public int page_start { get; set; }
        /// <summary>
        /// 100
        /// </summary>
        public int page_size { get; set; }
    }
    public class AddDepositRequest : BorsRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": هزینه هر واحد")]
        public long unit_cost_id { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": تعداد")]
        public int count { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": مبلغ")]
        public long amount { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("حساب")]
        public long account_id { get; set; }
        public List<AddDepositPurchaseRequest> purchases { get; set; }

        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            if (purchases == null ? true : purchases.Count == 0)
            {
                validation_errors.Add("حداقل یک خریدار الزامی است");
                is_valid = false;
            }
            else
            {
                is_valid = purchases.TrueForAll(x => x.CheckAttrbuteValidation());
            }
            return is_valid;
        }
        protected override bool CheckAccessValidation()
        {
            bool is_valid = true;
            var account = Startup.HSBorsDbContext.GetAccount(new Account { id = account_id }).Result;
            if (account == null ? true : account.accounter_id != UserSession.Id)
            {
                validation_errors.Add(Constants.MessageText.NoDataAccess);
                is_valid = false;
            }
            return is_valid;
        }
        public class AddDepositPurchaseRequest : BorsRequest
        {
            [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": خریدار")]
            public long buyer_id { get; set; }
            [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": مبلغ خرید")]
            public long amount { get; set; }
            public long? bank_copartner_id { get; set; }
            public int? bank_copartner_percent { get; set; }
            public int? bank_copartner_intrest { get; set; }
            public long? convention_id { get; set; }
            protected override bool CheckPropertyValidation()
            {
                bool is_valid = true;
                if (bank_copartner_id != null && (!(bank_copartner_percent > 0) || !(bank_copartner_intrest > 0)))
                {
                    is_valid = false;
                    validation_errors.Add("درصد و سود بانکی را تعیین نمایید");
                }
                return is_valid;
            }
        }
    }
    public class AddFundRequest : BorsRequest
    {
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام صندوق")]
        public string name { get; set; }
        public string no { get; set; }

    }
    public class AddUnitCostRequest : BorsRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صندوق")]
        public long fund_id { get; set; }
        [DisplayName("تاریخ اعلام هزینه"), RegularExpression(@"^1[34][0-9][0-9]\/((1[0-2])|([1-9]))\/(([12][0-9])|(3[01])|[1-9])$")]
        public string pdate { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("هزینه صدور")]
        public long issue_cost { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("هزینه ابطال")]
        public long cancel_cost { get; set; }

    }
    public class AddAccountRequest : BorsRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صندوق")]
        public long fund_id { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("نام حساب")]
        public string name { get; set; }
        public string no { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صاحب حساب")]
        public long accounter_id { get; set; }
        protected override bool CheckAccessValidation()
        {
            bool is_valid = true;
            if (accounter_id != UserSession.Id)
            {
                validation_errors.Add(Constants.MessageText.NoDataAccess);
                is_valid = false;
            }
            return is_valid;
        }

    }
    public class AddPurchaseRequest : AddDepositRequest.AddDepositPurchaseRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("سرمایه گذاری")]
        public long deposit_id { get; set; }
    }
    public class AddRoleRequest : BorsRequest
    {
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام")]
        public string name { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("دسترسی ها")]
        public List<string> accesses { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("کاربران")]
        public List<long> user_ids { get; set; }
        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            is_valid = accesses == null ? false : accesses.Count > 0;
            if (is_valid == false)
                validation_errors.Add(Constants.MessageText.RoleAccessNotDefinedError);
            else
            {
                is_valid = user_ids == null ? false : user_ids.Count > 0;
                if (is_valid == false)
                    validation_errors.Add(Constants.MessageText.RoleUsersNotDefinedError);
            }
            return is_valid;
        }


    }
    public class AddUserRequest : BorsRequest
    {

        internal PassMode pass_mode { get; set; }

        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام")]
        public string first_name { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام خانوادگی")]
        public string last_name { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("موبایل")]
        [SRL.Security.Mobile(ErrorMessage = Constants.MessageText.FieldFormatErrorDynamic)]
        public string mobile { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("کدملی")]
        [SRL.Security.NationalCode(ErrorMessage = Constants.MessageText.FieldFormatErrorDynamic)]
        public string national_code { get; set; }
        public string password { get; set; }
        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            if (pass_mode == PassMode.add || !string.IsNullOrWhiteSpace(password))
            {
                if (password == null) is_valid = false;
                else
                {
                    string pass = password.ToString();
                    if (pass.Length < 8) is_valid = false;
                }
            }
            if (is_valid == false)
                validation_errors.Add(Constants.MessageText.PasswordFormatError);

            return is_valid;
        }

    }
    public class AddGainerRequest : BorsRequest
    {

        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("کاربر اصلی")]
        public long primary_user_id { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("کاربر زیرمجموعه")]
        public long secondary_user_id { get; set; }
        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            if (primary_user_id == secondary_user_id)
            {
                validation_errors.Add(Constants.MessageText.AddRepeatedField);
                is_valid = false;
            }
            return is_valid;
        }

    }
    public class AddConventionRequest : BorsRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("طرف اول")]
        public long first_user_id { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("طرف دوم")]
        public long second_user_id { get; set; }
        [DisplayName("سود بانکی")]
        public int? bank_copartner_percent { get; set; }
        [DisplayName("درصد شراکت")]
        public int? bank_copartner_intrest { get; set; }

        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            if (first_user_id == second_user_id)
            {
                validation_errors.Add(Constants.MessageText.ContractorsEqualError);
                is_valid = false;
            }
            return is_valid;
        }

    }
    public class AddSettingRequest : BorsRequest
    {
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نوع")]
        public string type { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("عنوان")]
        public string key { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("مقدار")]
        public string value { get; set; }

    }
    public class SearchDepositRequest : PagedRequest
    {
        public long? unit_cost_id { get; set; }
        public long? account_id { get; set; }
        public int? status { get; set; }
        public long? fund_id { get; set; }
    }
    public class SearchFundRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? buyer_id { get; set; }
        public long? account_id { get; set; }
        public int? status { get; set; }
    }
    public class SearchAccountRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? fund_id { get; set; }
        public string status { get; set; }
    }
    public class SearchUserRequest : PagedRequest
    {
        public long? id { get; set; }
        public int? status { get; set; }
    }
    public class SearchGainerRequest : PagedRequest
    {
        public long? id { get; set; }
        public int? status { get; set; }
    }
    public class SearchConventionRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? first_user_id { get; set; }
        public long? second_user_id { get; set; }
    }
    public class SearchPurchaseRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? buyer_id { get; set; }
        public int? status { get; set; }
        public long? deposit_id { get; set; }
    }
    public class SearchUnitRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? fund_id { get; set; }
        public int? status { get; set; }
    }
    public static class RequestConvertor
    {
        public static Deposit ToEntity(this AddDepositRequest request, bool add_id)
        {
            Deposit deposit = new Deposit
            {
                unit_cost_id = request.unit_cost_id,
                account_id = request.account_id,
                amount = request.amount,
                create_date = DateTime.Now,
                count = request.count,
                creator_id = UserSession.Id,
                status = EntityStatus.active,
                update_type = UpdateType.add
            };
            if (add_id) deposit.id = request.id;
            return deposit;
        }
        public static List<Purchase> ToPurchaseEntity(this AddDepositRequest request, bool add_id = false)
        {
            List<Purchase> purchases = new List<Purchase>();
            foreach (var item in request.purchases)
            {
                Purchase purchase = new Purchase
                {
                    amount = item.amount,
                    buyer_id = item.buyer_id,
                    create_date = DateTime.Now,
                    creator_id = UserSession.Id
                };
                if (item.convention_id != null)
                {
                    purchase.convention_id = item.convention_id;
                }
                else if (item.bank_copartner_id != null)
                {
                    var conv = new Convention
                    {
                        first_user_id = item.buyer_id,
                        second_user_id = (long)item.bank_copartner_id,
                        bank_copartner_intrest = item.bank_copartner_intrest,
                        bank_copartner_percent = item.bank_copartner_percent,
                        create_date = DateTime.Now,
                        creator_id = UserSession.Id
                    };
                    purchase.convention = conv;
                }
                if (add_id) purchase.id = request.id;
                purchases.Add(purchase);
            }
            return purchases;

        }
        public static List<AddPurchaseRequest> ToPurchasesRequest(this AddDepositRequest request, bool add_id = false)
        {
            List<AddPurchaseRequest> purchases = new List<AddPurchaseRequest>();
            foreach (var item in request.purchases)
            {
                AddPurchaseRequest purchase = new AddPurchaseRequest
                {
                    amount = item.amount,
                    bank_copartner_id = item.bank_copartner_id,
                    bank_copartner_intrest = item.bank_copartner_intrest,
                    bank_copartner_percent = item.bank_copartner_percent,
                    buyer_id = item.buyer_id,
                    deposit_id = request.id,
                    convention_id = item.convention_id

                };
                if (add_id) purchase.id = item.id;
                purchases.Add(purchase);
            }
            return purchases;

        }
        public static Purchase ToEntity(this AddPurchaseRequest request, bool add_id = false)
        {
            Purchase purchase = new Purchase
            {
                deposit_id = request.deposit_id,
                amount = request.amount,
                buyer_id = request.buyer_id,
                convention_id = request.convention_id,
                create_date = DateTime.Now,
                creator_id = UserSession.Id,
                update_type = UpdateType.add
            };
            if (request.convention_id == null && request.bank_copartner_id != null)
            {
                purchase.convention = new Convention
                {
                    first_user_id = request.buyer_id,
                    second_user_id = (long)request.bank_copartner_id,
                    bank_copartner_intrest = request.bank_copartner_intrest,
                    bank_copartner_percent = request.bank_copartner_percent,
                };
            }
            if (add_id) purchase.id = request.id;
            return purchase;
        }
        public static Fund ToEntity(this AddFundRequest request)
        => new Fund
        {
            create_date = DateTime.Now,
            creator_id = UserSession.Id,
            name = request.name,
            no = request.no,
            status = EntityStatus.active,
            update_type = UpdateType.add

        };
        public static UnitCost ToEntity(this AddUnitCostRequest request, UpdateType update_type)
        => new UnitCost
        {
            create_date = DateTime.Now,
            creator_id = UserSession.Id,
            cancel_cost = request.cancel_cost,
            pdate = request.pdate,
            fund_id = request.fund_id,
            issue_cost = request.issue_cost,
            update_type = update_type
        };
        public static User ToEntity(this AddUserRequest request)
        {
            var user = new User
            {
                create_date = DateTime.Now,
                creator_id = UserSession.Id,
                first_name = request.first_name,
                last_name = request.last_name,
                mobile = request.mobile,
                status = EntityStatus.active,
                national_code = request.national_code,
                password = request.password,
                update_type = UpdateType.add
            };
            return user;
        }
        public static Setting ToEntity(this AddSettingRequest request, bool add_id = false)
        {
            var entity =
               new Setting
               {
                   create_date = DateTime.Now,
                   creator_id = UserSession.Id,
                   type = EnumConvert.StringToEnum<SettingType>(request.type),
                   key = request.key,
                   value = request.value,
                   status = EntityStatus.active,
                   update_type = UpdateType.add
               };
            if (add_id) entity.id = request.id;
            return entity;
        }
        public static Account ToEntity(this AddAccountRequest request)
     => new Account
     {
         create_date = DateTime.Now,
         creator_id = UserSession.Id,
         name = request.name,
         no = request.no,
         accounter_id = request.accounter_id,
         status = EntityStatus.active,
         fund_id = request.fund_id,
         update_type = UpdateType.add
     };
        public static Convention ToEntity(this AddConventionRequest request)
   => new Convention
   {
       create_date = DateTime.Now,
       creator_id = UserSession.Id,
       first_user_id = request.first_user_id,
       second_user_id = request.second_user_id,
       bank_copartner_percent = request.bank_copartner_percent,
       bank_copartner_intrest = request.bank_copartner_intrest,
       update_type = UpdateType.add
   };
        public static Role ToEntity(this AddRoleRequest request)
   => new Role
   {
       create_date = DateTime.Now,
       creator_id = UserSession.Id,
       accesses = request.accesses.Join(","),
       name = request.name,
       status = EntityStatus.active
   };
        public static Gainer ToEntity(this AddGainerRequest request)
 => new Gainer
 {
     primary_user_id = request.primary_user_id,
     secondary_user_id = request.secondary_user_id
 };
    }
#pragma warning restore CS1591
}