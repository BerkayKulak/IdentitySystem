using IdentitySystem.Models;
using IdentitySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentitySystem.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace IdentitySystem.Controllers
{
    [Authorize]// membercontrollere sadece üyeler erişecek.
    public class MemberController : BaseController
    {
       
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager,signInManager)
        {
          
        }


        public IActionResult Index()
        {
            // kullanıcı bir siteye geldiği zaman, üye de olsa olmasada user classı oluşturur
            // bir tane kimliği oluşur, eğer kullanıcı giriş yapmamışsa isauthenticate false olur.
            // boş bir kimlik oluşur. login olursa biz name,usernama gibi alanları yakalayabiliriz.
            //User.Identity.


            AppUser user = CurrentUser;
            // userin içindeki propertylerden UserViewModel içerisindeki Propertyler ile eşleşenleri
            // userViewModel'e aktaracak
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            //UserViewModel userViewModel = new UserViewModel();
            //userViewModel.UserName = user.UserName;


            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            // UserViewModel, AppUser'in kullanıcıya yansıyan tarafıydı
            AppUser user = CurrentUser;



            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(userViewModel);// kullanıcı bilgileri güncellicek bu yüzden UserViewModel'i dolu olarak gönderiyorum.
        }


        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel,IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if(userPicture!=null && userPicture.Length>0)
                {
                    //GetExtension, userPicture'in uzantısını alır jpg,png gibi
                    //Guid.NewGuid().ToString() yaparak isim oluşturuyoruz rastgele
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);

                    // wwwroot'un yolunu alıyorum.
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        // statik dosyaların hepsi wwwroot içinde olması gerekiyor.
                        user.Picture = "/UserPicture/" + fileName;
                    }

                }



                // güncelliyoruz.
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;
                user.BirthDay = userViewModel.BirthDay;
                user.Gender = (int)userViewModel.Gender;

                // startuptaki hatalar geçerli. burdaki hataları Update yaparken bir hata ile karşılaşırsa
                // bunu IdentityResult  resulta atacak
                // UpdateAsync hem custom validationları hem de startup tarafındaki validationları içeriyior//
                IdentityResult result = await userManager.UpdateAsync(user);

                if(result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);

                    // tekrar çıkış yaptı
                    await signInManager.SignOutAsync();

                    // true dememizin amacı cookie 60 gün geçerli olcak demek (60) günü belirtmiştik 

                    await signInManager.SignInAsync(user, true);

                    ViewBag.Success = "true";
                }
                else
                {
                    AddModelError(result);
                }


            }

            return View(userViewModel);

        }




        public IActionResult PasswordChange()
        {
            return View();
        }


        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {

            if (ModelState.IsValid)
            {
                // burdaki name değeri Cookie bilgisinden okuyor.
                AppUser user = CurrentUser;


                // eski şifresi doğru mu ilk bunu kontrol edelim.
                bool exist = userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;

                // şifre doğruysa yani şifre varsa
                if (exist)
                {
                    IdentityResult result = userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result;

                    // kullanıcı şifresi doğruysa direk Index ' e yönlendirebiliriz.
                    // ya da şifreniz değiştirildi diyebiliriz. biz böyle yapcaz.
                    if (result.Succeeded)
                    {
                        userManager.UpdateSecurityStampAsync(user);

                        // tekrar çıkış yaptı
                        signInManager.SignOutAsync();

                        // tekrar giriş yaptı. bunu kullanıcı hissetmicek ama cookiesi oluşmul olucak.
                        
                        signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true,false);

                        // eğer SignOutAsync,PasswordSignInAsync  yapmasaydım IdentityApi 30 dakika içinde sistemden atıcak ve login sayfasına yönlendiricek.

                        

                        ViewBag.success = "true";


                    }
                    else
                    {
                        AddModelError(result);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Eski şifreniz yanlış");
                }

            }


            //  ModelState.AddModelError("", "Eski şifreniz yanlış"); ile hataları ekledik
            // varsa bunları gösterebilmek için içine passwordChangeViewModel yazıyoruz.
            return View(passwordChangeViewModel);
        }


        public void LogOut()
        {
            signInManager.SignOutAsync();
      
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        [Authorize(Roles = "manager,Admin")]
        public IActionResult Manager()
        {
            // burdaki actiona artık sadece manager rolüne sahip olanlar girecek.
            return View();
        }


        [Authorize(Roles = "editor,Admin")]
        public IActionResult Editor()
        {
            // burdaki actiona artık sadece editor rolüne sahip olanlar girecek.
            return View();//456
        }

        [Authorize(Policy ="AnkaraPolicy")]
        public IActionResult AnkaraPage()
        {
            return View();
        }



    }
}
