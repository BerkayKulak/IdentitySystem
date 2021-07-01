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
    public class HomeController : Controller
    {
        public UserManager<AppUser> userManager { get; }
        public SignInManager<AppUser> signInManager { get; }
        public HomeController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Index()
        {
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
                    // eğer kullanıcı gerçekten başarılı bir şekilde kayıt olmuşsa 
                    // giriş ekranına yönlendiririm. biz kayıt olduktan sonra giriş ekranına yönlendireceğiz
                    // 2.senaryo

                    // LogIn ekranına gidecek
                    return RedirectToAction("LogIn");
                }
                // başarılı olmadıysa
                else
                {
                    foreach (IdentityError item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }

            // bu userviewmodeli şu hataları ekle kullanıcıya tekrar gönder.
            // kullanıcı değerleri girdikten sonra bir hata varsa 0 dan tekrar girmesin
            // girdiği değerlerle birlikte tekrar göngderiyorum. hata varsa hatalarıda gönderiyorum.
            return View(userViewModel);

        }


    }
}
