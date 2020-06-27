using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HSBors
{
    public class Constants
    {
        public class MessageText
        {
            public const string RequiredFieldError = "فیلد اجباری تکمیل نشده است";
            public const string RangeFieldError = "مقدار وارد شده معتبر نیست";
            public const string RangeFieldErrorDynamic = "مقدار وارد شده فیلد {0} معتبر نیست";
            public const string AdditionalParameter = "ورودی دارای اطلاعات اضافی است";
            public const string UnexpectedError = "خطای غیرمنتظره رخ داده است لطفا مجددا تلاش کنید یا با پشتیبان تماس بگیرید";
            public const string DbSaveNotDone = "اطلاعات ذخیره نشد مجددا تلاش کنید یا با پشتیبان تماس بگیرید";
            public const string RequiredFieldErrorDynamic = "فیلد اجباری {0} تکمیل نشده است";
            public const string ErrorNotSet = "خطا تعیین نشده است";
            public const string OperationOK = "عملیات با موفقیت انجام شد";
            public const string DbUpdateException = "خطا در ذخیره سازی اطلاعات";
            public const string AddRepeatedEntity = "اطلاعات تکراری است";
            public const string MobileFormatError = "فرمت موبایل اشتباه است";
            public const string NoContent = "مورد یافت نشد";
            public const string PurcheseAmountSumError = "جمع مبالغ سرمایه گذاران باید برابر مبلغ سرمایه گذاری باشد";

            public static string RequiredFieldErrorMes(string field) => $"{RequiredFieldError}: {field}";
        }
        public class PropName
        {
            public const string fund_id = "سرمایه گذاری"; 
        }
    }
}
