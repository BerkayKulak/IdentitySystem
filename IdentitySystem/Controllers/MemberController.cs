using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySystem.Controllers
{
    [Authorize]// membercontrollere sadece üyeler erişecek.
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
