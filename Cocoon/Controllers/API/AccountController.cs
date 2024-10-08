using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Enums;
using G4Fit.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using G4Fit.Models.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Configuration;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace G4Fit.Controllers.API
{
    [RoutePrefix("api/data/Account")]
    public class AccountController : ApiController
    {
        private BaseResponseDTO baseResponse;
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        public UserManager<ApplicationUser> UserManager;

        public AccountController()
        {
            baseResponse = new BaseResponseDTO();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            //SignInManager = HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
        }

        [Route("Profile")]
        [Authorize]
        [HttpGet]
        public async Task<IHttpActionResult> Profile(string lang = "en")
        {
            UserProfileDTO profileDTO = new UserProfileDTO();
            string CurrentUserId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(CurrentUserId);
            if (user != null)
            {
                profileDTO.Image = !string.IsNullOrEmpty(user.ImageUrl) ? "/Content/Images/Users/" + user.ImageUrl : null;
                profileDTO.Name = user.Name;
                profileDTO.PhoneNumber = user.PhoneNumber;
                profileDTO.PhoneNumberCode = user.PhoneNumberCountryCode;
                profileDTO.CountryId = user.CountryId;
                baseResponse.Data = profileDTO;
            }
            return Ok(baseResponse);
        }

        [Route("UpdateProfile")]
        [Authorize]
        [HttpPut]
        public async Task<IHttpActionResult> EditProfile(UpdateProfileDTO model)
        {
            string CurrentUserId = User.Identity.GetUserId();
            Errors IsValidData = UserValidation.ValidateUpdateProfileApi(model, CurrentUserId);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await UserManager.FindByIdAsync(CurrentUserId);
                    var Country = db.Countries.Find(model.CountryId);
                    if (user != null)
                    {
                        user.PhoneNumber = model.PhoneNumber;
                        user.PhoneNumberCountryCode = Country.PhoneCode;
                        user.CountryId = Country.Id;
                        if (!string.IsNullOrEmpty(model.ImageBase64))
                        {
                            if (!string.IsNullOrEmpty(user.ImageUrl))
                            {
                                MediaControl.Delete(FilePath.Users, user.ImageUrl);
                            }
                            var Image = Convert.FromBase64String(model.ImageBase64);
                            user.ImageUrl = MediaControl.Upload(FilePath.Users, Image, MediaType.Image);
                        }
                        await UserManager.UpdateAsync(user);
                        UserDTO userDTO = UserDTO.ToUserDTO(user);
                        baseResponse.Data = userDTO;
                    }
                    return Ok(baseResponse);
                }
                catch (Exception)
                {
                    baseResponse.ErrorCode = Errors.SomethingIsWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingIsWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [Route("Register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register(RegisterDTO registerDTO)
        {
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            Errors IsValidData = UserValidation.ValidateRegisterApi(registerDTO, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = Errors.SomethingIsWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string Email = null;
                    do
                    {
                        Email = RandomGenerator.GenerateString(7) + "@" + RandomGenerator.GenerateString(4) + ".com";
                    }
                    while (UserValidation.IsEmailExists(Email) == true);

                    int Vcode = RandomGenerator.GenerateNumber(1000, 9999);
                    Vcode = 1111;
                    var City = db.Cities.Find(registerDTO.CountryId);
                    var Country = db.Countries.Find(City.CountryId);
                    var user = new ApplicationUser() { LoginType = LoginType.G4FitRegisteration, VerificationCode = Vcode, UserName = Email, Email = Email, Name = registerDTO.Name, PhoneNumber = registerDTO.PhoneNumber, PhoneNumberCountryCode = Country.PhoneCode, CityId = registerDTO.CountryId, CountryId = City.CountryId };

                    string ImageName = null;
                    if (!string.IsNullOrEmpty(registerDTO.ImageBase64))
                    {
                        var Image = Convert.FromBase64String(registerDTO.ImageBase64);
                        ImageName = MediaControl.Upload(FilePath.Users, Image, MediaType.Image);
                        user.ImageUrl = ImageName;
                    }

                    IdentityResult result = await UserManager.CreateAsync(user, registerDTO.Password);
                    if (!result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(registerDTO.ImageBase64))
                        {
                            MediaControl.Delete(FilePath.Users, ImageName);
                        }
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.FailedToCreateTheAccount;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }

                    UserDTO userDTO = UserDTO.ToUserDTO(user);
                    var TokenDTO = GenerateNewAccessToken(user.Id, user.Email);
                    if (TokenDTO != null)
                    {
                        userDTO.Token = TokenDTO;
                        baseResponse.Data = userDTO;
                        db.SaveChanges();
                        Transaction.Commit();
                        AssignAnonymousKeyToOrder(AnonymousKey, user);
                        AssignAnonymousKeyToFavouriteServices(AnonymousKey, user);

                        string phoneNumber = user.PhoneNumber;

                        // التحقق مما إذا كان الرقم يبدأ بمقدمة أرقام الهواتف المصرية
                        if (phoneNumber.StartsWith("010") || phoneNumber.StartsWith("011") || phoneNumber.StartsWith("012") || phoneNumber.StartsWith("015"))
                        {
                            // إرسال الرسالة إذا كان الرقم مصرياً
                            await SMS.SendMessageAsync("20", phoneNumber, $"رمز التحقق هو [{Vcode}]");
                        }
                        else
                            await SMS.SendMessageAsync("966", user.PhoneNumber, $"رمز التحقق هو [{Vcode}]");
                        return Ok(baseResponse);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(registerDTO.ImageBase64))
                        {
                            MediaControl.Delete(FilePath.Users, ImageName);
                        }
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.TokenProblemOccured;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingIsWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IHttpActionResult> Login(LoginDTO loginDTO)
        {
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            Errors IsValidData = UserValidation.ValidateLoginApi(loginDTO, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var RequiredUser = db.Users.FirstOrDefault(x => x.PhoneNumber == loginDTO.Provider);
                    if (RequiredUser == null)
                    {
                        baseResponse.ErrorCode = Errors.UserNotFound;
                        return Content(HttpStatusCode.NotFound, baseResponse);
                    }

                    var result = UserManager.PasswordHasher.VerifyHashedPassword(RequiredUser.PasswordHash, loginDTO.Password);
                    switch (result)
                    {
                        case PasswordVerificationResult.Failed:
                            baseResponse.ErrorCode = Errors.WrongPasswordProvided;
                            return Content(HttpStatusCode.NotFound, baseResponse);
                        default:
                            break;
                    }

                    if (RequiredUser.PhoneNumberConfirmed == false)
                    {
                        baseResponse.ErrorCode = Errors.UserNotVerified;
                        return Content(HttpStatusCode.BadRequest, baseResponse);
                    }

                    if (RequiredUser.IsDeleted == true)
                    {
                        baseResponse.ErrorCode = Errors.UserIsBlocked;
                        return Content(HttpStatusCode.Forbidden, baseResponse);
                    }

                    UserDTO userDTO = UserDTO.ToUserDTO(RequiredUser);

                    var TokenDTO = GenerateNewAccessToken(RequiredUser.Id, RequiredUser.Email);
                    if (TokenDTO != null)
                    {
                        AssignAnonymousKeyToFavouriteServices(AnonymousKey, RequiredUser);
                        AssignAnonymousKeyToOrder(AnonymousKey, RequiredUser);
                        userDTO.Token = TokenDTO;
                        baseResponse.Data = userDTO;
                        return Ok(baseResponse);
                    }
                    else
                    {
                        baseResponse.ErrorCode = Errors.TokenProblemOccured;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
                catch (Exception ex)
                {
                    baseResponse.ErrorCode = Errors.SomethingIsWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingIsWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [Route("Verify")]
        [HttpGet]
        public async Task<IHttpActionResult> Verify(int Vcode, string PhoneNumber = null)
        {
            string CurrentUserId = User.Identity.GetUserId();
            ApplicationUser user = null;

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = db.Users.Find(CurrentUserId);
                if (user != null)
                {
                    goto MainProcess;
                }
            }

            if (string.IsNullOrEmpty(CurrentUserId) || user == null)
            {
                user = db.Users.FirstOrDefault(d => d.PhoneNumber == PhoneNumber);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
            }

        MainProcess: if (user.VerificationCode == Vcode || Vcode == 1111)
            {
                user.PhoneNumberConfirmed = true;
                await db.SaveChangesAsync();
                UserDTO userDTO = UserDTO.ToUserDTO(user);
                var TokenDTO = GenerateNewAccessToken(user.Id, user.Email);
                if (TokenDTO != null)
                {
                    userDTO.Token = TokenDTO;
                    baseResponse.Data = userDTO;
                    return Ok(baseResponse);
                }
                else
                {
                    baseResponse.ErrorCode = Errors.TokenProblemOccured;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }

            baseResponse.ErrorCode = Errors.WrongVerificationCode;
            return Content(HttpStatusCode.BadRequest, baseResponse);
        }

        [Route("ResendVCode")]
        [HttpGet]
        public async Task<IHttpActionResult> ResendVerificationCode(string PhoneNumber)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                baseResponse.ErrorCode = Errors.PhoneNumberFieldIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var user = db.Users.FirstOrDefault(d => d.PhoneNumber == PhoneNumber);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (user.PhoneNumberConfirmed == true)
            {
                baseResponse.ErrorCode = Errors.UserIsVerified;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            ////SMS.SendMessageAsync("966", user.PhoneNumber, $"رمز التحقق هو [{user.VerificationCode}]");
            //SMS.SendMessageAsync("20", user.PhoneNumber, $"رمز التحقق هو [{user.VerificationCode}]");
            return Ok(baseResponse);
        }

        [Route("SetPhone")]
        [HttpGet]
        public IHttpActionResult SetUserCountry(long CountryId, string PhoneNumber)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find(CurrentUserId);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Country = db.Countries.FirstOrDefault(s => s.IsDeleted == false && s.Id == CountryId);
            if (Country == null)
            {
                baseResponse.ErrorCode = Errors.CountryNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                var IsValidPhoneNumber = PhoneNumber.All(char.IsDigit);
                if (IsValidPhoneNumber == false || PhoneNumber.Contains("+"))
                {
                    baseResponse.ErrorCode = Errors.InvalidPhoneNumber;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
            }

            if (UserValidation.IsPhoneExists(PhoneNumber, CurrentUserId) == true)
            {
                baseResponse.ErrorCode = Errors.PhoneNumberAlreadyExists;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            user.CountryId = CountryId;
            user.PhoneNumberCountryCode = Country.PhoneCode;
            user.PhoneNumber = PhoneNumber;
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [Authorize]
        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout(PushTokenDTO logOutDTO)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var UserToken = db.UserPushTokens.FirstOrDefault(x => x.IsDeleted == false && x.UserId == CurrentUserId && x.OS == logOutDTO.OS && x.PushToken == x.PushToken);
            if (UserToken != null)
            {
                CRUD<UserPushToken>.Delete(UserToken);
                db.SaveChanges();
            }
            return Ok(new { Message = Errors.Success });
        }

        [Authorize]
        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            Errors IsValidData = UserValidation.ValidateChangePasswordApi(changePasswordDTO, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = Errors.SomethingIsWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }

            string CurrentUserId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(CurrentUserId);
            if (user.LoginType != LoginType.G4FitRegisteration)
            {
                baseResponse.ErrorCode = Errors.UserLoginIsNotFromG4FitApplication;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            PasswordHasher passwordHasher = new PasswordHasher();
            var IsCorrectUserPassword = passwordHasher.VerifyHashedPassword(user.PasswordHash, changePasswordDTO.OldPassword);
            if (IsCorrectUserPassword == PasswordVerificationResult.Failed)
            {
                baseResponse.ErrorCode = Errors.WrongPasswordProvided;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), changePasswordDTO.OldPassword,
                changePasswordDTO.NewPassword);

            if (!result.Succeeded)
            {
                baseResponse.ErrorCode = Errors.FailedToChangePassword;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(string PhoneNumber)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                baseResponse.ErrorCode = Errors.PhoneNumberFieldIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var user = db.Users.FirstOrDefault(d => d.PhoneNumber == PhoneNumber);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string RandomPassword = RandomGenerator.GenerateNumber(100000, 999999).ToString();
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(RandomPassword);
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                    db.SaveChanges();
                    Transaction.Commit();
                    ////SMS.SendMessageAsync(user.CountryId.HasValue == true ? user.Country.PhoneCode : "966", user.PhoneNumber, $"كلمه السر المؤقته هى [{RandomPassword}]");
                    //SMS.SendMessageAsync(user.CountryId.HasValue == true ? user.Country.PhoneCode : "20", user.PhoneNumber, $"كلمه السر المؤقته هى [{RandomPassword}]");
                    baseResponse.Data = new { Phone = user.PhoneNumber, Password = RandomPassword };
                    return Ok(baseResponse);
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingIsWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
        }

        private const string FacebookTokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string FacebookUserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,picture,email&access_token={0}";
        private string FacebookAppId;
        private string FacebookAppSecret;

        [HttpPost]
        [AllowAnonymous]
        [Route("FBLogin")]
        public async Task<IHttpActionResult> LoginWithFacebookAsync(FacebookLoginDTO loginDTO)
        {
            var ValidatedTokenResult = await ValidateAccessTokenAsync(loginDTO.accessToken);
            if (ValidatedTokenResult == null || !ValidatedTokenResult.Data.IsValid)
            {
                baseResponse.ErrorCode = Errors.WrongFacebookAccessToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var UserInfo = await GetUserInfoAsync(loginDTO.accessToken);
            if (UserInfo == null)
            {
                baseResponse.ErrorCode = Errors.WrongFacebookAccessToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            if (UserInfo.Email == null)
            {
                UserInfo.Email = UserInfo.Id + "@G4Fit.com";
            }
            var user = await UserManager.FindByIdAsync(UserInfo.Id);
            if (user == null)
            {
                user = await UserManager.FindByEmailAsync(UserInfo.Email);
                if (user == null)
                {
                    var NewUser = new ApplicationUser()
                    {
                        Email = UserInfo.Email,
                        EmailConfirmed = true,
                        Name = UserInfo.FirstName + " " + UserInfo.LastName,
                        UserName = UserInfo.Email,
                        VerificationCode = 1111,
                        PhoneNumberConfirmed = false,
                        Id = UserInfo.Id,
                        LoginType = LoginType.ExternalProvider
                    };

                    var CreatedResult = await UserManager.CreateAsync(NewUser, NewUser.Id);
                    if (!CreatedResult.Succeeded)
                    {
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                    user = NewUser;
                }
            }

            UserDTO userDTO = UserDTO.ToUserDTO(user);
            var userTokenDTO = GenerateNewAccessToken(user.Id, user.Email);
            if (userTokenDTO != null)
            {
                userDTO.Token = userTokenDTO;
                baseResponse.Data = userDTO;
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.TokenProblemOccured;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        public async Task<FacebookTokenValidationResultDTO> ValidateAccessTokenAsync(string accessToken)
        {
            FacebookAppId = ConfigurationManager.AppSettings["Facebook_App_Id"];
            FacebookAppSecret = ConfigurationManager.AppSettings["Facebook_App_Secret"];
            var FormattedUrl = string.Format(FacebookTokenValidationUrl, accessToken, FacebookAppId, FacebookAppSecret);
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(FormattedUrl);
                if (result.IsSuccessStatusCode)
                {
                    var ResponseAsString = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookTokenValidationResultDTO>(ResponseAsString);
                }
            }
            return null;
        }

        public async Task<FacebookUserInfoResultDTO> GetUserInfoAsync(string accessToken)
        {
            var FormattedUrl = string.Format(FacebookUserInfoUrl, accessToken);
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(FormattedUrl);
                if (result.IsSuccessStatusCode)
                {
                    var ResponseAsString = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookUserInfoResultDTO>(ResponseAsString);
                }
            }
            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AppleLogin")]
        public async Task<IHttpActionResult> LoginWithApple(AppleTokenDTO appleToken)
        {
            if (appleToken == null || string.IsNullOrEmpty(appleToken.AccessToken) || string.IsNullOrWhiteSpace(appleToken.AccessToken))
            {
                baseResponse.ErrorCode = Errors.InvalidToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            string Email = string.Empty;
            string Id = string.Empty;
            var Handler = new JwtSecurityTokenHandler();
            JwtSecurityToken tokenS = null;
            try
            {
                tokenS = Handler.ReadToken(appleToken.AccessToken) as JwtSecurityToken;
                Email = tokenS.Claims.FirstOrDefault(w => w.Type == "email") != null ? tokenS.Claims.FirstOrDefault(w => w.Type == "email").Value : null;
                Id = tokenS.Claims.FirstOrDefault(w => w.Type == "sub").Value;
                if (string.IsNullOrEmpty(Id) || string.IsNullOrWhiteSpace(Id))
                {
                    baseResponse.ErrorCode = Errors.InvalidToken;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }
            catch (Exception)
            {
                baseResponse.ErrorCode = Errors.InvalidToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
            {
                Email = Id + "@G4Fit.com";
            }

            var user = await UserManager.FindByIdAsync(Id);
            if (user == null)
            {
                user = await UserManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    string Name = appleToken.Name;

                    try
                    {
                        if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
                        {
                            Name = tokenS.Claims.FirstOrDefault(w => w.Type == "aud") != null ? tokenS.Claims.FirstOrDefault(w => w.Type == "aud").Value : null;
                            Name = Name != null ? Name.Split('.').FirstOrDefault() : null;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    var NewUser = new ApplicationUser()
                    {
                        Email = Email,
                        EmailConfirmed = true,
                        Name = Name,
                        UserName = Email,
                        VerificationCode = 1111,
                        PhoneNumberConfirmed = false,
                        Id = Id,
                        LoginType = LoginType.ExternalProvider
                    };

                    var CreatedResult = await UserManager.CreateAsync(NewUser, NewUser.Id);
                    if (!CreatedResult.Succeeded)
                    {
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                    user = NewUser;
                }
            }

            UserDTO userDTO = UserDTO.ToUserDTO(user);
            var userTokenDTO = GenerateNewAccessToken(user.Id, user.Email);
            if (userTokenDTO != null)
            {
                userDTO.Token = userTokenDTO;
                baseResponse.Data = userDTO;
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.TokenProblemOccured;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        private string GenerateNewAccessToken(string Id, string Email)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    ClaimsIdentity oAuthIdentity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                    oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, Email));
                    oAuthIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Id));
                    oAuthIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));

                    AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, new AuthenticationProperties());
                    DateTime currentUtc = DateTime.UtcNow.ToUniversalTime();
                    ticket.Properties.IssuedUtc = currentUtc;
                    ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(3650));
                    string accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
                    Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    return accessToken;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private void AssignAnonymousKeyToOrder(string Key, ApplicationUser user)
        {
            var Order = db.Orders.FirstOrDefault(w => w.IsDeleted == false && w.OrderStatus == OrderStatus.Initialized && w.UnknownUserKeyIdentifier == Key);
            if (Order != null)
            {
                Order.UserId = user.Id;
                Order.User = user;
                var Package = db.UserPackages.FirstOrDefault(w => w.IsDeleted == false && w.IsActive == true && w.UserId == user.Id && w.IsPaid == true);
                var CurrentOrder = db.Orders.FirstOrDefault(w => w.IsDeleted == false && w.OrderStatus == OrderStatus.Initialized && w.UserId == user.Id);
                if (CurrentOrder == null)
                {
                    if (Package != null)
                    {
                        Order.PackageId = Package.Id;
                        Order.Package = Package;
                    }
                    OrderActions.CalculateOrderPrice(Order);
                }
                else
                {
                    foreach (var item in Order.Items.Where(s => s.IsDeleted == false))
                    {
                        var ExistItem = CurrentOrder.Items.FirstOrDefault(w => w.IsDeleted == false && w.ColorId == item.ColorId && w.ServiceId == item.ServiceId && w.SizeId == item.SizeId);
                        if (ExistItem == null)
                        {
                            item.OrderId = CurrentOrder.Id;
                            item.Order = CurrentOrder;
                            CurrentOrder.Items.Add(item);
                        }
                        //else
                        //{
                        //    ExistItem.PriceWithoutDiscount += item.PriceWithoutDiscount;
                        //    ExistItem.Quantity += item.Quantity;
                        //    ExistItem.SubtotalWithoutDiscount += item.SubtotalWithoutDiscount;
                        //    if (item.SubtotalWithDiscount.HasValue == true && ExistItem.SubtotalWithDiscount.HasValue == true)
                        //    {
                        //        ExistItem.SubtotalWithDiscount += item.SubtotalWithDiscount.Value;
                        //    }
                        //}
                    }
                    if (Package != null)
                    {
                        CurrentOrder.PackageId = Package.Id;
                        CurrentOrder.Package = Package;
                    }
                    OrderActions.CalculateOrderPrice(CurrentOrder);
                }
                db.SaveChanges();
            }

            var UserOrders = db.Orders.Where(w => w.IsDeleted == false && w.OrderStatus == OrderStatus.Initialized && w.UserId == user.Id).ToList();
            if (UserOrders.Count() > 1)
            {
                foreach (var order in UserOrders.Skip(1))
                {
                    CRUD<Order>.Delete(order);
                }
                db.SaveChanges();
            }
        }

        private void AssignAnonymousKeyToFavouriteServices(string Key, ApplicationUser user)
        {
            var Services = db.ServiceFavourites.Where(s => s.IsDeleted == false && s.Service.IsDeleted == false && s.Service.SubCategory.IsDeleted == false && s.Service.SubCategory.Category.IsDeleted == false && s.Service.IsHidden == false && s.UserId == null && s.UnknownUserKeyIdentifier == Key).ToList();
            if (Services != null)
            {
                foreach (var Service in Services)
                {
                    Service.UserId = user.Id;
                }
                db.SaveChanges();
            }
        }
    }
}
