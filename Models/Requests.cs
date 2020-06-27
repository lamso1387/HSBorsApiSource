using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HSBors.Models
{
#pragma warning disable CS1591
    public class DateRange : RangeAttribute
    {
        public DateRange()
           : base(typeof(DateTime), DateTime.Now.AddYears(-20).ToShortDateString(), DateTime.Now.AddYears(20).ToShortDateString()) { }
    }
    public abstract class BorsRequest
    {
        public long id { get; set; }
        public bool CheckValidation(IResponse response)
        { 

            string request_error = null;
            if(CheckAttrbuteValidation())
            {
                if (!CheckPropertyValidation())
                    request_error = validation_errors.First();
            }
            else request_error= validation_errors.First();

            if (request_error != null)
            {
                response.ErrorCode = (int)ErrorHandler.ErrorCode.BadRequest;
                response.ErrorMessage = request_error;
                return false;
            }
            return true;
        }
        public bool CheckAttrbuteValidation()
        {
            validation_errors = SRL.ClassManagement.CheckValidationAttribute(this);
            return validation_errors.Count == 0 ? true : false;
        }
        protected List<string> validation_errors { get; set; }
        protected virtual bool CheckPropertyValidation() { return true; }
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
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": هزینه هر واحد")]
        public long unit_cost_id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": تعداد")]
        public int count { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": مبلغ")]
        public long amount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("حساب")]
        public long account_id { get; set; }
        public List<AddDepositPurchaseRequest> purchases { get; set; }

        protected override bool CheckPropertyValidation()
        {
            bool is_valid = true;
            if (purchases ==null? true : purchases.Count == 0)
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

        public class AddDepositPurchaseRequest : BorsRequest
        {
            [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": خریدار")]
            public long buyer_id { get; set; }
            [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldError + ": مبلغ خرید")]
            public long amount { get; set; }
            public long? bank_copartner_id { get; set; }
            public int? bank_copartner_percent { get; set; }
            public int? bank_copartner_intrest { get; set; }
            protected override bool CheckPropertyValidation()
            {
                bool is_valid = true;
                if (bank_copartner_id != null && (!(bank_copartner_percent > 0) || !(bank_copartner_intrest > 0)))
                {
                    is_valid = false;
                    validation_errors.Add("درصد و سود بانکی را تعیین نماپید");
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
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صندوق")]
        public long fund_id { get; set; }
        [DateRange(ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("تاریخ اعلام هزینه")]
        public DateTime date { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("هزینه صدور")]
        public long issue_cost { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("هزینه ابطال")]
        public long cancel_cost { get; set; }

    }
    public class AddAccountRequest : BorsRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صندوق")]
        public long fund_id { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("نام حساب")]
        public string name { get; set; }
        public string no { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName("صاحب حساب")]
        public long accounter_id { get; set; }

    }
    public class AddPurchaseRequest : AddDepositRequest.AddDepositPurchaseRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = Constants.MessageText.RangeFieldErrorDynamic), DisplayName(Constants.PropName.fund_id)]
        public long deposit_id { get; set; }
    }
    public class AddUserRequest : BorsRequest
    {
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام")]
        public string first_name { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("نام خانوادگی")]
        public string last_name { get; set; }
        [Required(ErrorMessage = Constants.MessageText.RequiredFieldErrorDynamic), DisplayName("موبایل")]
        [SRL.Security.Mobile(ErrorMessage = Constants.MessageText.MobileFormatError)]
        public string mobile { get; set; }

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
    public class SearchPurchaseRequest : PagedRequest
    {
        public long? id { get; set; }
        public int? status { get; set; }
        public long? deposit_id { get; set; }
    }
    public class SearchUnitRequest : PagedRequest
    {
        public long? id { get; set; }
        public long? fund_id { get; set; }
        public int? status { get; set; }
    }
    public static class Extensions
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
                creator_id = 1,
                status = EntityStatus.active
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
                    bank_copartner_id = item.bank_copartner_id,
                    bank_copartner_intrest = item.bank_copartner_intrest,
                    bank_copartner_percent = item.bank_copartner_percent,
                    buyer_id = item.buyer_id,
                    create_date = DateTime.Now,
                    creator_id = 1
                };
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
                    deposit_id=request.id
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
                bank_copartner_id = request.bank_copartner_id,
                bank_copartner_intrest = request.bank_copartner_intrest,
                bank_copartner_percent = request.bank_copartner_percent,
                buyer_id = request.buyer_id,
                create_date = DateTime.Now,
                creator_id = 1
            };
            if (add_id) purchase.id = request.id; 
            return purchase;
        }
        public static Fund ToEntity(this AddFundRequest request)
        => new Fund
        {
            create_date = DateTime.Now,
            creator_id = 1,
            name = request.name,
            no = request.no,
            status = EntityStatus.active

        };
        public static UnitCost ToEntity(this AddUnitCostRequest request)
        => new UnitCost
        {
            create_date = DateTime.Now,
            creator_id = 1,
            cancel_cost = request.cancel_cost,
            date = request.date,
            fund_id = request.fund_id,
            issue_cost = request.issue_cost
        };
        public static User ToEntity(this AddUserRequest request)
     => new User
     {
         create_date = DateTime.Now,
         creator_id = 1,
         first_name = request.first_name,
         last_name = request.last_name,
         mobile = request.mobile,
         status = EntityStatus.active,
         username = request.first_name,
         password = "test"
     };
        public static Setting ToEntity(this AddSettingRequest request, bool add_id = false)
        {
            var entity =
               new Setting
               {
                   create_date = DateTime.Now,
                   creator_id = 1,
                   type = EnumConvert.StringToEnum<SettingType>(request.type),
                   key = request.key,
                   value = request.value,
                   status = EntityStatus.active
               };
            if (add_id) entity.id = request.id;
            return entity;
        }
        public static Account ToEntity(this AddAccountRequest request)
     => new Account
     {
         create_date = DateTime.Now,
         creator_id = 1,
         name = request.name,
         no = request.no,
         accounter_id = request.accounter_id,
         status = EntityStatus.active,
         fund_id = request.fund_id
     };
    }
#pragma warning restore CS1591
}