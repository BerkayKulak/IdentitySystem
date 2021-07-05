using IdentitySystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySystem.CustomTagHelpers
{
    //Users.cshtml deki user-rolesi yakalamasını istiyoruz.
    // hem tdsi olan hemde user-roles sahip olan tagi yakala dedim
    [HtmlTargetElement("td",Attributes = "user-roles")]
    public class UserRolesName:TagHelper
    {
        public UserManager<AppUser> userManager { get; set; }
        public UserRolesName(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        //user-rolesine gidecek onun içindeki @item.Id kısmını benim burdaki UserId ye bind edecek
        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //elimde UserId olduğu için useri bulabilirim.
            AppUser user = await userManager.FindByIdAsync(UserId);

            //userin hangi rollere sahip olduğunu alacağız
            // elimde artık roller var html kodlarını yazabiliriz artık
            IList<string> roles = await userManager.GetRolesAsync(user);

            string html = string.Empty;

            roles.ToList().ForEach(x =>
            {
                html += $"<span class ='badge badge-info'> {x} </span>";

            });

            output.Content.SetHtmlContent(html);


        }

    }
}
