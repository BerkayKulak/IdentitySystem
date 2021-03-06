using IdentitySystem.Models;
using IdentitySystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySystem.Controllers
{
    public class HomeController : BaseController
    {
      

        
        public HomeController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager):base(userManager, signInManager)
        {
           
        }
        public IActionResult Index()
        {
            // kullanıcı login olmuşsa önceden bu sayfayı göster direk olarak
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
            return View();
        }


        public IActionResult LogIn(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel userlogin)
        {

            if(ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(userlogin.Email);

                if(user!=null)
                {

                    if(await userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                        return View(userlogin);
                    }

                    if(userManager.IsEmailConfirmedAsync(user).Result == false)
                    {
                        ModelState.AddModelError("", "Email adresiniz doğrulanmamıştır. Lütfen epostanızı kontrol ediniz.");
                        return View(userlogin);
                    }


                   await signInManager.SignOutAsync();

                    //userlogin.RememberMe = cookienin gerçekten geçerli olup olmadığını kontrol edicez. checkboxtan kontrol edicez. işaretlersem true olur.
                    // benim startuptaki 60 gün geçerli olacak
                    Microsoft.AspNetCore.Identity.SignInResult result =  await signInManager.PasswordSignInAsync(user, userlogin.Password, userlogin.RememberMe, false);

                    if(result.Succeeded)
                    {
                        // başarılı giriş yaptığımız için AccessFailedCount değerini 0 lıcak
                        await userManager.ResetAccessFailedCountAsync(user);

                        if(TempData["ReturnUrl"]!=null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else 
                    {
                        // başarısız girişte 1 artıcak
                        await userManager.AccessFailedAsync(user);

                       

                        // kaç başarısız giriş yaptı alır.
                        int fail = await userManager.GetAccessFailedCountAsync(user);

                        ModelState.AddModelError("", $"{fail} kez başarısız giriş.");

                        if (fail == 3)
                        {
                            // kullanıcıyı 20 dakka kilitliyoruz
                            await userManager.SetLockoutEndDateAsync(user, new System.DateTimeOffset(DateTime.Now.AddMinutes(20)));


                            ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 20 dakika süreyle kilitlenmiştir.");
                        }

                        else
                        {
                            ModelState.AddModelError("", "Email adresi veya şifre Yanlış");

                        }


                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayıtlı kullanıcı bulunamamıştır.");
                }
            }
            return View(userlogin);
        }



        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            // gelen verilerin doğru olup olmadığını kontrol etmek
            // backend tarafında hem client tarafında kontrol yapıyprum ajaxda 
            // bunu otomatik olarak jquerry gerçekleştirecek bizim mvc mimarisi gereği

            // olurda kullanıcı javascript özelliğini kapatırsa client tarafında doğrulama yapamam
            // ben bu doğrulamayı backend tarafında yapmam lazım


            if(ModelState.IsValid)
            {
                if(userManager.Users.Any(u=>u.PhoneNumber == userViewModel.PhoneNumber))
                {
                    ModelState.AddModelError("", "Bu telefon numarası kayıtlıdır.");

                    return View(userViewModel);
                }


                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                // şuanda şifreyi eklemicem çünkü plan text olarak gelir. bunu hashliceğimizden getirmiyoruz.
                // await derkei bak kardeşim bu metod yani bu satır bitmeden alt satıra geçme, sonucu ata ve alta geç


                // IdentityResult bize = üye oluştururken bir hata olursa biz bunu result üzerinden yakalayabileceğiz.


                IdentityResult result = await userManager.CreateAsync(user, userViewModel.Password);

                // 1.senaryo bazı web siteleri ilk kayıt işlemi gerçekleştiğinde aynı zamanda login işlemide gerçekleştiriyor
                // 2.senaryo bazı web siteleri kullanıı üye olduktan sonra kullanıcıyı login ekranına yönlendiriyor.
                if (result.Succeeded)
                {
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken


                    },protocol:HttpContext.Request.Scheme);

                    Helper.EmailConfirmation.SendEmail(link, user.Email);

                        

                    // eğer kullanıcı gerçekten başarılı bir şekilde kayıt olmuşsa 
                    // giriş ekranına yönlendiririm. biz kayıt olduktan sonra giriş ekranına yönlendireceğiz
                    // 2.senaryo

                    // LogIn ekranına gidecek
                    return RedirectToAction("LogIn");
                }
                // başarılı olmadıysa
                else
                {
                    AddModelError(result);
                }
            }

            // bu userviewmodeli şu hataları ekle kullanıcıya tekrar gönder.
            // kullanıcı değerleri girdikten sonra bir hata varsa 0 dan tekrar girmesin
            // girdiği değerlerle birlikte tekrar göngderiyorum. hata varsa hatalarıda gönderiyorum.
            return View(userViewModel);

        }

        public IActionResult ResetPassword()
        {
            return View();
        }


        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel passwordResetViewModel)
        {
            // benim veritabanımda kayıtlı kullanıcı var mı onu tespit edelim önce
            AppUser user = userManager.FindByEmailAsync(passwordResetViewModel.Email).Result;

            if(user != null)
            {
                // userManager.GeneratePasswordResetTokenAsync(user) bunu yaptığımız zaman
                // user bilgilerinden oluşan bir tane token oluşuyor.

                string passwordResetToken = userManager.GeneratePasswordResetTokenAsync(user).Result;

                string passwordResetLink = Url.Action("ResetPasswordConfirm","Home",new { 
                
                    userId = user.Id,
                    token = passwordResetToken
                
                },HttpContext.Request.Scheme);


                // www.bıdıbıdı.com/Home/ResetPasswordConfirm?userId = asdfd&token = adgsg

                Helper.PasswordReset.PasswordResetSendEmail(passwordResetLink,user.Email);

                ViewBag.status = "success";

            }
            else
            {
                ModelState.AddModelError("", "Sistemde kayıtlı email adresi bulunamamıştır.");

            }


            return View();
        }


        public IActionResult ResetPasswordConfirm(string userId,string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }


        [HttpPost]

        // Bind = PasswordResetViewModel classına hangi değerlerin geleceğini belirtiyoruz. Emaili belirtmedik mesela
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")]PasswordResetViewModel passwordResetViewModel)
        {
            //TempData = sayfalar arası veri taşımak için kullanıyoruz.
            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();

            AppUser user = await userManager.FindByIdAsync(userId);

            if(user!=null)
            {
                // şifrem sıfırlanacak
                IdentityResult result = await userManager.ResetPasswordAsync(user, token, passwordResetViewModel.PasswordNew);

                // başarılıysa 0 lanmış demektir.
                // SecurityStampi Update edecez
                // SecurityStamp = kullanıcının bilgileriye alakalı bir o anki anlık durumu tutan bir stampti
                // önemli bir bilgiyi değiştirdiğimiz zaman veritabanında SecurityStampi de değiştiriyoruz
                // mesela telefon numarası değişiyorsa gerek yok, özellikle username,password gibi alanlarda değiştir.

                if (result.Succeeded)
                {
                    // bunu yapmazsak eski şifreyle dolaşmaya devam eder.
                    await userManager.UpdateSecurityStampAsync(user);

                  


                    ViewBag.status = "success";
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Hata meydana gelmiştir. Lütfen daha sonra tekrar deneyiniz.");
            }

            return View(passwordResetViewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);

            if(result.Succeeded)
            {
                ViewBag.status = "Email adressiniz onaylanmıştır. Login ekranından giriş yapabilirsiniz.";
            }
            else
            {
                ViewBag.status = "Bir hata meydana geldi. Lütfen daha sonra tekrar deneyiniz.";
            }


            return View();

        }


    }
}
