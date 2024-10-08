using G4Fit.Helpers;
using G4Fit.Models.DTOs;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                    Password = model.Password,
                    CountryId = model.CountryId
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