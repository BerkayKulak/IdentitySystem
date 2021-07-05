using IdentitySystem.Models;
using IdentitySystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
namespace IdentitySystem.Controllers
{
    public class AdminController : BaseController
    {
        

        public AdminController(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager):base(userManager,null,roleManager)
        {
           
        }

        public IActionResult Index()
        {
        
            return View();
        }

        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel roleViewModel)
        {
            AppRole role = new AppRole();
            role.Name = roleViewModel.Name;
            IdentityResult result = roleManager.CreateAsync(role).Result;

            if(result.Succeeded)
            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }

            return View(roleViewModel);
        }

        public IActionResult Roles()
        {
            return View(roleManager.Roles.ToList());
        }

        public IActionResult Users()
        {
            // veritabanındaki usersları çektik ve listeye attık
            return View(userManager.Users.ToList());
        }

        // asp-route-id="@item.Id" dan dolayı string id belirtiyorum.
        public IActionResult RoleDelete(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;
            if(role!=null)
            {
                IdentityResult result =  roleManager.DeleteAsync(role).Result;

             
            }


            return RedirectToAction("Roles");
        }

        public IActionResult RoleUpdate(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;

            if(role !=null)
            { 
                // role içerisinden gelen(AppRole) Id ve name kısmı
                // benim RoleViewModeldeki Id ve name kısmı ile eşleşir.
                return View(role.Adapt<RoleViewModel>());
               
            }
            return RedirectToAction("Roles");

        }


        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            // böyle bir ıd var mı onu güncellicez.
            AppRole role = roleManager.FindByIdAsync(roleViewModel.Id).Result;
            if(role!=null)
            {
                role.Name = roleViewModel.Name;
                IdentityResult result = roleManager.UpdateAsync(role).Result;

                if(result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result),
                }

            }
            else
            {
                ModelState.AddModelError("", "Güncelleme işlemi başarısız oldu.");
            }
            return View(roleViewModel);

        }
    }
}
