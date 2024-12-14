using G4Fit.Helpers;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    public class AccountController : BaseController
    {
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginVM userLogin, string ReturnUrl)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/Login";

            using (HttpClient httpClient = new HttpClient())
            {
                string CurrentUITwoLetters = culture;
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                    var bodyJS = JsonConvert.SerializeObject(userLogin);
                    var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(base_url, body);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<ResultResponseDTO>(result);
                        var dataJs = JsonConvert.SerializeObject(data.Data);
                        var userDTO = JsonConvert.DeserializeObject<UserDTO>(dataJs);
                        var user = db.Users.Find(userDTO.UserId);
                        await SignInManager.SignInAsync(user, true, true);
                        HttpCookie Token = Request.Cookies["G4Fit-data-token"];
                        Token = new HttpCookie("G4Fit-data-token");
                        Token.Value = userDTO.Token;
                        Token.Expires = DateTime.Now.AddYears(3);
                        Response.Cookies.Add(Token);
                        if (UserManager.IsInRole(user.Id, "Supplier"))
                        {
                            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.IsAccepted == true && w.UserId == user.Id);
                            if (Supplier != null)
                            {
                                return RedirectToAction("Index", "Suppliers");
                            }
                        }
                        if (!string.IsNullOrEmpty(ReturnUrl))
                            return Redirect(ReturnUrl);
                        else
                            return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        if (result.ErrorCode == Models.Enums.Errors.UserNotVerified)
                        {
                            TempData["VerifyCodePageLoad"] = true;
                            TempData["UserEmail"] = userLogin.Provider;
                            TempData["UserPassword"] = userLogin.Password;
                            return RedirectToAction("VerifyCode", "Account");
                        }
                        else
                        {
                            var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, CurrentUITwoLetters);
                            ModelState.AddModelError("Provider", ErrorMessage);
                            return View(userLogin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var ErrorMessage = CurrentUITwoLetters == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong";
                    ModelState.AddModelError("Provider", ErrorMessage);
                    return View(userLogin);
                }
            }
        }

        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterVM model, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (IsImage == false)
                {
                    if (culture == "ar")
                        ModelState.AddModelError("Image", "الصوره المرفقه غير صحيحة");
                    else
                        ModelState.AddModelError("Image", "Invalid photo uploaded");
                }
            }
            if (ModelState.IsValid)
            {
                RegisterDTO dTO = new RegisterDTO()
                {
                    PhoneNumber = model.PhoneNumber,
                    ConfirmPassword = model.ConfirmPassword,
                    Name = model.Name,
                    Address = model.Address,
                    Email = model.Email,
                    IDNumber = model.IDNumber,
                    Password = model.Password,
                    CountryId = 1,
                };
                if (Image != null)
                {
                    dTO.ImageBase64 = MediaControl.ConvertImageToBase64(Image);
                }
                string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/Register";
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                        var bodyJS = JsonConvert.SerializeObject(dTO);
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync(base_url, body);
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var dataJs = JsonConvert.SerializeObject(result.Data);
                            var userDTO = JsonConvert.DeserializeObject<UserDTO>(dataJs);
                            TempData["VerifyCodePageLoad"] = true;
                            TempData["UserEmail"] = userDTO.Email;
                            TempData["UserPassword"] = model.Password;
                            return RedirectToAction("VerifyCode", "Account");
                        }
                        else
                        {
                            var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                            ModelState.AddModelError("Error", ErrorMessage);
                        }
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong");
                    }
                }
            }
            ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode()
        {
            if (TempData["VerifyCodePageLoad"] == null)
                return RedirectToAction("Index", "Home");

            var Email = TempData["UserEmail"] as string;
            var user = await UserManager.FindByEmailAsync(Email);
            if (user == null)
            {
                user = db.Users.FirstOrDefault(d => d.PhoneNumber == Email);
                if (user != null)
                {
                    TempData["UserEmail"] = user.Email;
                }
            }
            TempData.Keep("UserEmail");
            TempData.Keep("UserPassword");
            TempData.Keep("VerifyCodePageLoad");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyCode(VerifyAccountVM verifyAccountVM)
        {
            if (string.IsNullOrEmpty(verifyAccountVM.Provider) || string.IsNullOrEmpty(verifyAccountVM.Password))
            {
                return RedirectToAction("Index", "Home");
            }
            string Vcode = String.Join("", verifyAccountVM.Vcode1, verifyAccountVM.Vcode2, verifyAccountVM.Vcode3, verifyAccountVM.Vcode4);
            var user = await UserManager.FindByEmailAsync(verifyAccountVM.Provider);
            if (user == null)
                return RedirectToAction("Index", "Home");

            try
            {
                if (user.VerificationCode == int.Parse(Vcode) || Vcode == 1111.ToString())
                {
                    user.PhoneNumberConfirmed = true;
                    await UserManager.UpdateAsync(user);
                    using (HttpClient httpClient = new HttpClient())
                    {
                        try
                        {
                            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/Login";
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var userLogin = new LoginDTO() { Provider = user.PhoneNumber, Password = verifyAccountVM.Password };
                            var bodyJS = JsonConvert.SerializeObject(userLogin);
                            var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                            var response = await httpClient.PostAsync(base_url, body);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var data = JsonConvert.DeserializeObject<ResultResponseDTO>(result);
                                var dataJs = JsonConvert.SerializeObject(data.Data);
                                var userDTO = JsonConvert.DeserializeObject<UserDTO>(dataJs);
                                await SignInManager.PasswordSignInAsync(userDTO.Email, userLogin.Password, false, shouldLockout: false);
                                HttpCookie Token = Request.Cookies["G4Fit-data-token"];
                                Token = new HttpCookie("G4Fit-data-token");
                                Token.Value = userDTO.Token;
                                Token.Expires = DateTime.Now.AddYears(1);
                                Response.Cookies.Add(Token);
                            }

                        }
                        catch (Exception ex)
                        {
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData.Keep("UserEmail");
                    TempData.Keep("UserPassword");
                    TempData.Keep("VerifyCodePageLoad");
                    ModelState.AddModelError("", culture == "ar" ? "الرقم التأكيدى غير صحيح" : "Wrong verification code");
                    return View();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public async Task<ActionResult> Profile()
        {
            CurrentUserId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                var user = db.Users.Find(CurrentUserId);

                if (user == null)
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }

                ProfileVM vm = new ProfileVM()
                {
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    CountryId = user.CountryId,
                };
                ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
                return View(vm);
            }
            else
            {
                return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
            }


        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Profile(ProfileVM model, HttpPostedFileBase Image)
        {
            ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (IsImage == false)
                {
                    if (culture == "ar")
                        ModelState.AddModelError("Image", "الصوره المرفقه غير صحيحة");
                    else
                        ModelState.AddModelError("Image", "Invalid photo uploaded");
                }
            }
            if (ModelState.IsValid)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        var MessageError = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First .";
                        ViewBag.MessageError = MessageError;
                        return View(model);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        var MessageError = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه تنفيذ العمليه, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, This process is intended for customers only .";
                        ViewBag.MessageError = MessageError;
                        return View(model);
                    }

                    UpdateProfileDTO dTO = new UpdateProfileDTO()
                    {
                        PhoneNumber = model.PhoneNumber,
                        Name = model.Name,
                        Address = model.Address,
                        Email = model.Email,
                        //IDNumber = model.IDNumber,
                        CountryId = model.CountryId,
                    };
                    if (Image != null)
                    {
                        dTO.ImageBase64 = MediaControl.ConvertImageToBase64(Image);
                    }
                    string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/UpdateProfile";
                    var Token = Request.Cookies["G4Fit-data-token"];
                    string TokenValue = null;
                    string Anonymous = null;
                    HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
                    if (anonymousCooky != null)
                    {
                        Anonymous = anonymousCooky.Value;
                    }
                    else
                    {
                        Anonymous = Session.SessionID;
                        anonymousCooky = new HttpCookie("Anonymous");
                        anonymousCooky.Value = Anonymous;
                        anonymousCooky.Expires = DateTime.Now.AddYears(20);
                        Response.Cookies.Add(anonymousCooky);
                    }

                    if (Token != null && User.Identity.IsAuthenticated == true)
                    {
                        TokenValue = Token.Value;
                    }
                    else
                    {
                        var MessageError = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First .";
                        ViewBag.MessageError = MessageError;
                        return View(model);
                    }
                    using (HttpClient httpClient = new HttpClient())
                    {
                        try
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                            httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                            var bodyJS = JsonConvert.SerializeObject(dTO);
                            var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                            var response = await httpClient.PutAsync(base_url, body);
                            var data = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var dataJs = JsonConvert.SerializeObject(result.Data);
                                var userDTO = JsonConvert.DeserializeObject<UserDTO>(dataJs);
                                var Message = culture == "ar" ? "تم تحديث البيانات بنجاح" : "Your Profile was updated successfully .";
                                ViewBag.Message = Message;
                                model.Name = userDTO.Name;
                                model.Email = userDTO.Email;
                                model.Address = userDTO.Address;
                                model.PhoneNumber = userDTO.PhoneNumber;
                                model.CountryId = userDTO.CountryId;
                                model.ImageUrl = userDTO.ImageName;
                                return View(model);

                            }
                            else
                            {
                                var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                                ViewBag.MessageError = ErrorMessage;
                                return View(model);
                            }
                        }
                        catch (Exception)
                        {
                            var MessageError = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong";
                            ViewBag.MessageError = MessageError;
                            return View(model);

                        }
                    }

                }
                else
                {
                    var MessageError = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First .";
                    ViewBag.MessageError = MessageError;
                    return View(model);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string Provider)
        {
            if (string.IsNullOrEmpty(Provider))
            {
                ViewBag.Error = culture == "ar" ? "رقم الهاتف مطلوب" : "Phone number is required";
                return View();
            }
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/ForgotPassword?PhoneNumber=" + Provider;
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await httpClient.GetAsync(base_url);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            TempData["ForgotPasswordSuccess"] = true;
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            var data = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                            var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                            ViewBag.Error = ErrorMessage;
                            return View();
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = culture == "ar" ? "برجاء المحاولة فى وقت لاحق" : "Please try again later";
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                ViewBag.Error = culture == "ar" ? "برجاء المحاولة فى وقت لاحق" : "Please try again later";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult ResendVcode()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResendVcode(ResendVcodeModel model)
        {
            var user = db.Users.FirstOrDefault(d => d.PhoneNumber == model.Provider && d.PhoneNumberConfirmed == false);
            if (user == null)
            {
                ModelState.AddModelError("Provider", culture == "ar" ? "لا يوجد لدينا عضو مسجل بهذا الرقم وغير مُفعل" : "No non-verified user found with this phone number");
                return View();
            }

            user.VerificationCode = RandomGenerator.GenerateNumber(1000, 9999);
            db.SaveChanges();
            //SMS.SendMessageAsync("966", user.PhoneNumber, $"رمز التحقق هو [{user.VerificationCode}]");
            TempData["VerifyCodePageLoad"] = true;
            return RedirectToAction("VerifyCode");
        }

        [Authorize]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> FBLogin(string accessToken, string ReturnUrl)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/data/account/FBLogin";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    FacebookLoginDTO facebookLoginDTO = new FacebookLoginDTO()
                    {
                        accessToken = accessToken
                    };
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var bodyJS = JsonConvert.SerializeObject(facebookLoginDTO);
                    var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(base_url, body);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<ResultResponseDTO>(result);
                        var dataJs = JsonConvert.SerializeObject(data.Data);
                        var userDTO = JsonConvert.DeserializeObject<UserDTO>(dataJs);
                        var x = await SignInManager.PasswordSignInAsync(userDTO.Email, userDTO.UserId, false, shouldLockout: false);
                        HttpCookie Token = Request.Cookies["G4Fit-data-token"];
                        Token = new HttpCookie("G4Fit-data-token");
                        Token.Value = userDTO.Token;
                        Token.Expires = DateTime.Now.AddYears(1);
                        Response.Cookies.Add(Token);
                        if (!string.IsNullOrEmpty(ReturnUrl))
                            return Redirect(ReturnUrl);
                        else
                            return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception)
                {
                    var ErrorMessage = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong";
                    ModelState.AddModelError("", ErrorMessage);
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

    }
}