using G4Fit.Models.Data;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.ModelBinding;
using static QRCoder.PayloadGenerator;

namespace G4Fit.Helpers
{
    public static class UserValidation
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static string EmailSyntax = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        public static bool IsEmail(string Email)
        {
            bool isEmail = Regex.IsMatch(Email, EmailSyntax, RegexOptions.IgnoreCase);
            return isEmail;
        }

        public static bool IsEmailExists(string Email, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower());
            else
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower() && x.Id != CurrentUserId);
        }

        public static bool IsPhoneExists(string Phone, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.PhoneNumber == Phone);
            else
                return db.Users.Any(x => x.PhoneNumber == Phone && x.Id != CurrentUserId);
        }
        public static bool IsIDNumberExists(string IDNumber, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.IDNumber == IDNumber);
            else
                return db.Users.Any(x => x.IDNumber == IDNumber && x.Id != CurrentUserId);
        }

        public static Errors ValidateLoginApi(LoginDTO loginDTO, ModelStateDictionary Model)
        {
            if (loginDTO == null)
            {
                return Errors.LoginDataIsRequired;
            }
            var ModelErrors = Model.Values.SelectMany(v => v.Errors.Where(x => !string.IsNullOrEmpty(x.ErrorMessage))).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the provider field is required"))
                {
                    return Errors.ProviderFieldIsRequired;
                }
                else if (ModelErrors[i].ToLower().Contains("the password field is required"))
                {
                    return Errors.PasswordFieldIsRequired;
                }
                else if (ModelErrors[i].ToLower().Contains("the field password must be a string with a minimum length"))
                {
                    return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;
                }
                else
                    return Errors.SomethingIsWrong;
            }

            var IsValidPassword = loginDTO.Password.All(char.IsDigit);
            if (!IsValidPassword)
            {
                return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;
            }

            if (ModelErrors == null || ModelErrors.Count <= 0)
            {
                var IsValidPhoneNumber = loginDTO.Provider.All(char.IsDigit);
                if (IsValidPhoneNumber == false || loginDTO.Provider.Contains("+"))
                {
                    return Errors.InvalidPhoneNumber;
                }
            }

            return Errors.Success;
        }

        public static Errors ValidateRegisterApi(RegisterDTO registerDTO, ModelStateDictionary Model)
        {
            if (registerDTO == null)
            {
                return Errors.RegisterDataIsRequired;
            }

            if (!string.IsNullOrEmpty(registerDTO.PhoneNumber))
            {
                var IsValidPhoneNumber = registerDTO.PhoneNumber.All(char.IsDigit);
                //if (IsValidPhoneNumber == false || registerDTO.PhoneNumber.Contains("+"))
                //{
                //    return Errors.InvalidPhoneNumber;
                //}
                // التأكد أن رقم الجوال السعودي يبدأ بـ 05 ويتكون من 10 أرقام
                string pattern = @"^05[0-9]{8}$";
                if (!Regex.IsMatch(registerDTO.PhoneNumber, pattern))
                    return Errors.InvalidPhoneNumber;
            }
            if (!string.IsNullOrEmpty(registerDTO.IDNumber))
            {
                var IsValidIDNumber = registerDTO.IDNumber.All(char.IsDigit);
                //if (IsValidIDNumber == false || registerDTO.IDNumber.Length != 14)
                //{
                //    return Errors.InvalidIDNumber;
                //}
                // التأكد أن رقم الهوية السعودية يتكون من 10 أرقام
                string pattern = @"^\d{10}$";
                if (!Regex.IsMatch(registerDTO.IDNumber, pattern))
                    return Errors.InvalidIDNumber;
            }


            var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the name field is required"))
                    return Errors.NameFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the countryid field is required"))
                    return Errors.CountryIdFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the phonenumber field is required"))
                    return Errors.PhoneNumberFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the password field is required"))
                    return Errors.PasswordFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the field password must be a string with a minimum length of 6 and a maximum length of 6"))
                    return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;

                else if (ModelErrors[i].ToLower().Contains("the password and confirmation password do not match"))
                    return Errors.PasswordAndConfirmationPasswordDoNotMatch;

                else
                    return Errors.SomethingIsWrong;
            }

            //var IsValidPassword = registerDTO.Password.All(char.IsDigit);
            //if (!IsValidPassword)
            //{
            //    return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;
            //}

            var Country = db.Countries.FirstOrDefault(s => s.IsDeleted == false && s.Id == registerDTO.CountryId);
            if (Country == null)
            {
                return Errors.CountryNotFound;
            }

            if (IsPhoneExists(registerDTO.PhoneNumber))
                return Errors.PhoneNumberAlreadyExists;
            if (IsIDNumberExists(registerDTO.IDNumber))
                return Errors.IDNumberAlreadyExists;
            if (IsEmailExists(registerDTO.Email))
                return Errors.EmailAlreadyExists;

            if (!string.IsNullOrEmpty(registerDTO.ImageBase64))
            {
                try
                {
                    var Image = Convert.FromBase64String(registerDTO.ImageBase64);
                    if (Image == null || Image.Length <= 0)
                    {
                        return Errors.FailedToUploadImage;
                    }
                }
                catch (Exception)
                {
                    return Errors.FailedToUploadImage;
                }
            }
            return Errors.Success;
        }

        public static Errors ValidateUpdateProfileApi(UpdateProfileDTO updateProfileDTO, string currentUserId)
        {
            if (updateProfileDTO == null)
            {
                return Errors.ProfileDataIsRequired;
            }

            if (string.IsNullOrEmpty(updateProfileDTO.PhoneNumber))
            {
                return Errors.PhoneNumberFieldIsRequired;
            }
            else
            {
                var IsValidPhoneNumber = updateProfileDTO.PhoneNumber.All(char.IsDigit);
                //if (IsValidPhoneNumber == false || updateProfileDTO.PhoneNumber.Contains("+"))
                //{
                //    return Errors.InvalidPhoneNumber;
                //}
                string pattern = @"^05[0-9]{8}$";
                if (!Regex.IsMatch(updateProfileDTO.PhoneNumber, pattern))
                    return Errors.InvalidPhoneNumber;
            }

            if (string.IsNullOrEmpty(updateProfileDTO.Name))
            {
                return Errors.NameFieldIsRequired;
            }

            if (!string.IsNullOrEmpty(updateProfileDTO.ImageBase64))
            {
                try
                {
                    var Image = Convert.FromBase64String(updateProfileDTO.ImageBase64);
                    if (Image == null || Image.Length <= 0)
                    {
                        return Errors.FailedToUploadImage;
                    }
                }
                catch (Exception)
                {
                    return Errors.FailedToUploadImage;
                }
            }

            if (IsPhoneExists(updateProfileDTO.PhoneNumber, currentUserId) == true)
            {
                return Errors.PhoneNumberAlreadyExists;
            }
            if (IsEmailExists(updateProfileDTO.Email, currentUserId))
                return Errors.EmailAlreadyExists;

            var Country = db.Countries.FirstOrDefault(s => s.IsDeleted == false);
            if (Country == null)
            {
                return Errors.CountryNotFound;
            }

            return Errors.Success;
        }

        //public static Errors ValidateSetNewPasswordApi(NewPasswordDTO newPasswordDTO, ModelStateDictionary Model)
        //{
        //    if (newPasswordDTO == null)
        //    {
        //        return Errors.NewPasswordDataIsRequired;
        //    }

        //    var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //    for (int i = 0; i < ModelErrors.Count; i++)
        //    {
        //        if (ModelErrors[i].ToLower().Contains("the password field is required"))
        //            return Errors.PasswordFieldIsRequired;

        //        else if (ModelErrors[i].ToLower().Contains("the field password must be a string with a minimum length of 6 and a maximum length of 100"))
        //            return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;

        //        else if (ModelErrors[i].ToLower().Contains("the password and confirmation password do not match"))
        //            return Errors.PasswordAndConfirmationPasswordDoNotMatch;

        //        else if (ModelErrors[i].ToLower().Contains("the id field is required"))
        //            return Errors.ForgetPasswordIdIsRequired;

        //        else
        //            return Errors.SomethingIsWrong;
        //    }
        //    return Errors.Success;
        //}

        public static Errors ValidateChangePasswordApi(ChangePasswordDTO changePasswordDTO, ModelStateDictionary Model)
        {
            if (changePasswordDTO == null)
            {
                return Errors.ChangePasswordDataIsRequired;
            }

            var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the oldpassword field is required"))
                    return Errors.OldPasswordFieldIsRequired;

                if (ModelErrors[i].ToLower().Contains("the newpassword field is required"))
                    return Errors.NewPasswordFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the field newpassword must be a string with a minimum length of 6 and a maximum length of 100"))
                    return Errors.PasswordFieldMustBeStringWithMinimumLengthOf6Chars;

                else if (ModelErrors[i].ToLower().Contains("the new password and confirmation password do not match"))
                    return Errors.PasswordAndConfirmationPasswordDoNotMatch;

                else
                    return Errors.SomethingIsWrong;
            }
            return Errors.Success;
        }

        //public static Errors ValidateForgetPasswordApi(ForgotPasswordDTO forgotPasswordDTO, ModelStateDictionary Model)
        //{
        //    if (forgotPasswordDTO == null)
        //    {
        //        return Errors.ForgetPasswordDataIsRequired;
        //    }

        //    var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //    for (int i = 0; i < ModelErrors.Count; i++)
        //    {
        //        if (ModelErrors[i].ToLower().Contains("the phone field is required"))
        //            return Errors.PhoneFieldIsRequired;
        //        else
        //            return Errors.SomethingIsWrong;
        //    }

        //    bool IsValidPhoneNumber = IsPhoneNumber(forgotPasswordDTO.Phone);
        //    if (IsValidPhoneNumber == false)
        //    {
        //        return Errors.InvalidPhoneNumber;
        //    }

        //    return Errors.Success;
        //}
    }
}