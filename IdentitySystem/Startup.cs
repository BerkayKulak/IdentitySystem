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
            
            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "MyBlog";
            // k�t� niyetli kullan�c�lar client side tarafta benim cookime eri�emez.
            // http iste�i �zerinden cookie bilgisini almak istiyorum.
            cookieBuilder.HttpOnly = false;
            // s�re belirtelim. ne kadar s�re kullan�c�n�n bilgisayar�nda kals�n
            // cookie 60 g�n boyunca kalacak, login olduktan sonra 60 g�n gezinebilecek. sonra tekrar login olams� laz�m
            // cookieBuilder.Expiration = System.TimeSpan.FromDays(60);
            // sadece benim sitem �zerinden gelen cookie ayarlar�n� al
            cookieBuilder.SameSite = SameSiteMode.Lax;
            // always dersek browser sizin cookiesini , sadece bir https �zerinden bir istek gelmi�se g�nderiyor.
            // SameAsRequest dersek, e�er bu cookie bilgisi http �zerinden gelmi�se http den g�nderiyor
            // https derseniz htpps �zerinden g�nderir
            // None dersek isterse https olsun ister http olsun hepsini http �zeirnden getiriyor.
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            services.ConfigureApplicationCookie(opts =>
            {
               
                // kullan�c� �ye olmadan, �yelerin eri�ebildi�i bir sayfaya t�klarsa kullan�c�y� login sayfas�na y�nlendiririz.
                opts.LoginPath = new PathString("/Home/Login");
                opts.LogoutPath = new PathString("/Member/LogOut");

                opts.Cookie = cookieBuilder;

                // kullan�c�y�a 60 g�n vermi�tik ya hani, e�er siz SlidingExpiration s�resini true yaparsan�z.
                // 60'�n yar�s�n� ge�tikten sonra e�er siteye istek yaparsa tekrar bi 60 g�n daha eklicek.
                opts.SlidingExpiration = true;

                opts.ExpireTimeSpan = TimeSpan.FromDays(60);

                // e�er kullan�c� �ye olduktan sonra, admin linkine t�klarsa, editor rol�ne sahip ama y�netici rol�ne t�klarsa 
                // bu sayfaya eri�emedi�iyle ilgili bir bilgi verilir. Eri�ime yetkisi olmayan �ye kullan�c�lar�n gidece�i path olacak
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied");

            });

            services.AddMvc(options => options.EnableEndpointRouting = false);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            app.UseDeveloperExceptionPage();// sayfam�zda bir hata ald���m�zda a��klay�c� bilgiler sunar
            app.UseStatusCodePages();// hatan�n nerde oldu�unu g�steren bir yaz� g�steriyor.
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();// arka planda //Controller/Action/{id}

           



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
