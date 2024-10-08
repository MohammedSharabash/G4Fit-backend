using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Helpers
{
    public class APIResponseValidation
    {
        public static string Validate(Errors ErrorCode, string culture = "ar")
        {
            switch (ErrorCode)
            {
                case Errors.Success:
                    return culture == "ar" ? "تم بنجاح" : "Success";
                case Errors.RegisterDataIsRequired:
                    return culture == "ar" ? "بيانات تسجيل الحساب مطلوبة" : "Registeration data is required";
                case Errors.InvalidPhoneNumber:
                    return culture == "ar" ? "رقم الهاتف غير صحيح" : "Invalid phone number";
                case Errors.NameFieldIsRequired:
                    return culture == "ar" ? "الاسم مطلوب" : "Name field is required";
                case Errors.PhoneNumberFieldIsRequired:
                    return culture == "ar" ? "رقم الهاتف مطلوب" : "Phone number field is required";
                case Errors.PasswordFieldIsRequired:
                    return culture == "ar" ? "كلمه السر مطلوبة" : "Password field is required";
                case Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars:
                    return culture == "ar" ? "كلمه السر يجب ان تكون من 6 ارقام" : "Password field must be 6 digits";
                case Errors.PasswordAndConfirmationPasswordDoNotMatch:
                    return culture == "ar" ? "كلمه السر غير متطابقة" : "Password does not match";
                case Errors.PhoneNumberAlreadyExists:
                    return culture == "ar" ? "رقم الهاتف مسجل لدينا من قبل" : "Phone number has been registered before";
                case Errors.SomethingIsWrong:
                case Errors.FailedToCreateTheAccount:
                case Errors.TokenProblemOccured:
                case Errors.InvalidOperatingSystem:
                case Errors.ProfileDataIsRequired:
                case Errors.InvalidToken:
                    return culture == "ar" ? "برجاء المحاولة فى وقت لاحق" : "Please try again later";
                case Errors.LoginDataIsRequired:
                    return culture == "ar" ? "بيانات تسجيل الدخول مطلوبة" : "Login data is required";
                case Errors.ProviderFieldIsRequired:
                    return culture == "ar" ? "معرف الدخول مطلوب" : "Provider field is required";
                case Errors.UserNotFound:
                    return culture == "ar" ? "لا يوجد حساب مسجل بهذه البيانات" : "User not found";
                case Errors.WrongPasswordProvided:
                    return culture == "ar" ? "كلمه السر غير صحيحة" : "Wrong password";
                case Errors.UserNotVerified:
                    return culture == "ar" ? "الحساب لم يتم تفعيله بعد" : "User is not verified";
                case Errors.VerificationCodeFieldIsRequired:
                    return culture == "ar" ? "كود تفعيل الحساب مطلوب" : "Verification code field is required";
                case Errors.WrongVerificationCode:
                    return culture == "ar" ? "كود تفعيل الحساب غير صحيح" : "Verification code is wrong";
                case Errors.UserIsVerified:
                    return culture == "ar" ? "الحساب تم تفعيله" : "User is verified";
                case Errors.FailedToUploadImage:
                    return culture == "ar" ? "خطا فى حفظ الصورة" : "Failed to upload the image";
                case Errors.WrongFacebookAccessToken:
                    return culture == "ar" ? "خطا فى الدخول بالفيسبوك" : "Failed to login with facebook";
                case Errors.ChangePasswordDataIsRequired:
                    return culture == "ar" ? "بيانات تغيير كلمه السر مطلوبة" : "Change password data is required";
                case Errors.OldPasswordFieldIsRequired:
                    return culture == "ar" ? "كلمه السر الحالية مطلوبة" : "Old password field is required";
                case Errors.NewPasswordFieldIsRequired:
                    return culture == "ar" ? "كلمه السر الجديدة مطلوبة" : "New password field is required";
                case Errors.FailedToChangePassword:
                    return culture == "ar" ? "لم يتم تغيير كلمه السر ، برجاء المحاولة مره اخرى" : "Failed to change password, please try again later";
                case Errors.PageMustBeGreaterThanZero:
                    return culture == "ar" ? "رقم الصفحة الحالية غير صحيح" : "Page number is not valid";
                case Errors.ServiceNotFound:
                    return culture == "ar" ? "الخدمه المطلوب غير متاح" : "Service is not found";
                case Errors.UserIdentityIsRequired:
                case Errors.UserNotAuthorized:
                    return culture == "ar" ? "برجاء تسجيل الدخول" : "User is not authorized, please login";
                case Errors.SizeNotFound:
                    return culture == "ar" ? "الحجم المطلوب غير متاح" : "Size is not found";
                case Errors.ColorNotFound:
                    return culture == "ar" ? "اللون المطلوب غير متاح" : "Color is not found";
                case Errors.BasketItemNotFound:
                    return culture == "ar" ? "عنصر الطلب غير متاح" : "Basket item is not found";
                case Errors.CannotDecrease:
                    return culture == "ar" ? "لا يمكن تقليل العدد" : "Cannot decrease the item quantity";
                case Errors.UserBasketIsEmpty:
                    return culture == "ar" ? "لا يوجد خدمات فى عربه التسوق" : "User basket is empty";
                case Errors.OrderNotFound:
                    return culture == "ar" ? "الطلب غير متاح" : "Order is not found";
                case Errors.CategoryNotFound:
                    return culture == "ar" ? "القسم المطلوب غير متاح" : "Category is not found";
                case Errors.CountryIdFieldIsRequired:
                    return culture == "ar" ? "الدولة مطلوبة" : "Country field is required";
                case Errors.CountryNotFound:
                    return culture == "ar" ? "الدولة المطلوبة غير متاحه" : "Country is not found";
                case Errors.UserLoginIsNotFromG4FitApplication:
                    return culture == "ar" ? "لا يمكن تغيير كلمه السر لهذا الحساب" : "You cannot change password for this account";
                case Errors.PromoNotFound:
                    return culture == "ar" ? "الكوبون المطلوب غير صحيح" : "Coupon doesn't exist";
                case Errors.UserDoesNotHaveCountry:
                    return culture == "ar" ? "برجاء اختيار دوله المستخدم اولاً" : "Please choose a country first";
                case Errors.PromoExpired:
                    return culture == "ar" ? "الكوبون المطلوب قد انتهى" : "Coupon has been expired";
                case Errors.PromoCodeCannotBeAppliedToCurrentUser:
                    return culture == "ar" ? "هذا الكوبون غير صالح لهذا المستخدم" : "Coupon is not available for this account";
                case Errors.UserReachedItsLimitInUsingThePromoCode:
                    return culture == "ar" ? "لقد وصلت الى الحد الادنى المسموح لاستخدام هذا الكوبون" : "You have reached the coupon's limit";
                case Errors.OrderCostDidnotMeatMinimumPromoCodeRequiredCost:
                    return culture == "ar" ? "لم تصل الى الحد الادنى المطلوب من سعر الطلب لتفعيل الكوبون" : "Order price doesn't meet the coupon minimum value";
                case Errors.UserIsBlocked:
                    return culture == "ar" ? "عفواً ، هذا الحساب قد تم حظره" : "This account has been blocked";
                default:
                    break;
            }
            return null;
        }

    }
}