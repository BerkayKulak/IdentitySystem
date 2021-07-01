using IdentitySystem.CustomValidation;
using IdentitySystem.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySystem
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940


        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // istemi� oldu�u s�n�f�n bir nesne �rne�ini olu�turur.
            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionString"]);

            });

            //IdentityUseri' App user olarak miras ald�k.
            // IdentityRole ile miras alma i�lemi ger�ekle�tirmedi�imizden kullan�yoruz.

            // ok i�areti ��kartt���m zaman
            // bana gidip o ctordan istemi� oldu�u classtan bir tane class olu�turuyor.
            services.AddIdentity<AppUser, AppRole>(opts => {

                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters
                = "abc�defgh�ijklmno�pqrs�tu�vwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

                opts.Password.RequiredLength = 4; // default olarak en az 4 karakter isticez.
                opts.Password.RequireNonAlphanumeric = false; // y�ld�z ya da nokta gibi karakter istemiyor
                opts.Password.RequireLowercase = false; // k���k harf istemiyorum
                opts.Password.RequireUppercase = false; // b�y�k harf istemiyorum.
                opts.Password.RequireDigit = false;// say�sal karakter de istemiyorum 
            
            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddEntityFrameworkStores<AppIdentityDbContext>();



            services.AddMvc(options => options.EnableEndpointRouting = false);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            app.UseDeveloperExceptionPage();// sayfam�zda bir hata ald���m�zda a��klay�c� bilgiler sunar
            app.UseStatusCodePages();// hatan�n nerde oldu�unu g�steren bir yaz� g�steriyor.
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();// arka planda //Controller/Action/{id}

            app.UseAuthentication();
















            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("hello world!");
                });
            });



        }
    }
}
