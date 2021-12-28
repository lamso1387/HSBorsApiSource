using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HSBors
{
    public enum NonActionAccess
    {
        [DisplayName("اطلاعات شخصی")]
        MyData,
        [DisplayName("اطلاعات سهام داران من")]
        MyShareData,
        [DisplayName("همه اطلاعات")]
        AllData

    }
    public enum PassMode
    {
        none = 0,
        add = 1,
        edit = 2
    }
    public enum HSViewMode
    {
        ReadOnly = 0,
        Edit = 1,
        Insert = 2,
        ExcelInsert = 3
    }
    public enum EntityStatus
    {
        inactive = 0,
        active = 1
    }
    public enum UpdateType
    {
        add = 0,
        add_override = 1,
        edit = 2
    }
    public enum SettingType
    {
        AnnualProfit
    }
    public enum EventType
    {
        Call = 10,
        Return = 11,
        Exception = 12,
        Operation = 13
    }

    public enum ErrorCode
    {
        [Description(@"{""message"":""عملیات با موفقیت انجام شد"" ,""status"": ""OK""}")]
        OK = 0,
        [Description(@"{""message"":""ورودی اشتباه است"" ,""status"": ""BadRequest""}")]
        BadRequest = 1,
        [Description(@"{""message"":""خطای غیرمنتظره رخ داده است لطفا مجددا تلاش کنید یا با پشتیبان تماس بگیرید"" ,""status"": ""InternalServerError""}")]
        UnexpectedError = 2,
        [Description(@"{""message"":""اطلاعات ذخیره نشد مجددا تلاش کنید یا با پشتیبان تماس بگیرید"" ,""status"": ""ExpectationFailed""}")]
        DbSaveNotDone = 3,
        [Description(@"{""message"":""خطا در ذخیره سازی اطلاعات"" ,""status"": ""UnprocessableEntity""}")]
        DbUpdateException = 4,
        [Description(@"{""message"":""اطلاعات تکراری است"" ,""status"": ""Conflict""}")]
        AddRepeatedEntity = 5,
        [Description(@"{""message"":""مورد یافت نشد"" ,""status"": ""NoContent""}")]
        NoContent = 6,
        [Description(@"{""message"":""هزینه ابطال صندوق تعیین نشده است"" ,""status"": ""PreconditionFailed""}")]
        FundCancelCostNotSet = 7,
        [Description(@"{""message"":""سود سالانه تعیین نشده است"" ,""status"": ""PreconditionFailed""}")]
        AnnualProfitNotSet = 8,
        [Description(@"{""message"":""دسترسی به اطلاعات وجود ندارد"" ,""status"": ""Forbidden""}")]
        NoDataAccess = 9,
        [Description(@"{""message"":""نام کاربری یا رمز عبور اشتباه است"" ,""status"": ""Unauthorized""}")]
        Unauthorized = 10
    }
}
