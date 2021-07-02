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
namespace IdentitySystem.Controllers
{
    [Authorize]// membercontrollere sadece üyeler erişecek.
    public class MemberController : Controller
    {
        public UserManager<AppUser> userManager { get; }
        public SignInManager<AppUser> signInManager { get; }
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        public IActionResult Index()
        {
            // kullanıcı bir siteye geldiği zaman, üye de olsa olmasada user classı oluşturur
            // bir tane kimliği oluşur, eğer kullanıcı giriş yapmamışsa isauthenticate false olur.
            // boş bir kimlik oluşur. login olursa biz name,usernama gibi alanları yakalayabiliriz.
            //User.Identity.


            AppUser user = userManager.FindByNameAsync(User.Identity.Name).Result;
            // userin içindeki propertylerden UserViewModel içerisindeki Propertyler ile eşleşenleri
            // userViewModel'e aktaracak
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            //UserViewModel userViewModel = new UserViewModel();
            //userViewModel.UserName = user.UserName;


            return View(userViewModel);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }
    }
}
