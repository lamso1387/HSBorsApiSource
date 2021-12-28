using HSBors.Controllers;
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
            public const string RequiredFieldErrorDynamic = "فیلد اجباری {0} تکمیل نشده است";
            public const string ErrorNotSet = "خطا تعیین نشده است";
            public const string AddRepeatedField = "مقادیر فیلد ورودی باید متمایز باشد";
            public const string FieldFormatErrorDynamic = "فرمت {0} اشتباه است";
            public const string PasswordFormatError = "فرمت رمز عبور اشتباه است";
            public const string PurcheseAmountSumError = "جمع مبالغ سرمایه گذاران باید برابر مبلغ سرمایه گذاری باشد";
            public const string ContractorsEqualError = "طرفین قرارداد نمی توانند یکسان باشند";
            public const string RoleAccessNotDefinedError = "دسترسی ها مشخص نشده است";
            public const string RoleUsersNotDefinedError = "کاربران مشخص نشده اند";
            public const string NoDataAccess = "دسترسی به اطلاعات وجود ندارد";

            public static string RequiredFieldErrorMes(string field) => $"{RequiredFieldError}: {field}";
        } 
        public class Setting
        {
            public const string HsborsDbConfugrationValue = "AppSettings:HSBorsConnectionString";
        }
        public class Actions
        {
            public static string[] NoAuth = {  nameof(DepositController.TestNoDb) , nameof(UserController.AuthenticatePost) };
        }
    }
}
