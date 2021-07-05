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
    }
}
