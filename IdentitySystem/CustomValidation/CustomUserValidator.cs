using IdentitySystem.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySystem.CustomValidation
{
    public class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();
            string[] Digits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (var item in Digits)
            {
                // sayısal karakter içeriyorsa hata döndür.
                if(user.UserName[0].ToString()==item)
                {
                    errors.Add(new IdentityError()
                    {

                        Code = "UserNameContainsFirstLetterDigitContains",
                        Description = "Kullanıcı adının ilk karakteri sayısal karakter içeremez"

                    });
                }
            }

            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            else
            {
                // failed başarısız demek ama bizden bir Identity Error isminde bir array istiyor
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }
    }
}
