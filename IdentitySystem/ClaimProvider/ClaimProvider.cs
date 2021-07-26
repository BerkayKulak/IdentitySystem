﻿using IdentitySystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentitySystem.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        public UserManager<AppUser> userManager { get; set; }

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if(principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

                AppUser user = await userManager.FindByNameAsync(identity.Name);

                if(user!=null)
                {

                    if (user.City!=null)
                    {
                        if(!principal.HasClaim(c=>c.Type == "city"))
                        {
                            Claim cityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");

                            identity.AddClaim(cityClaim);
                        }
                    }


                }

            }


            return principal;
        }
    }
}
