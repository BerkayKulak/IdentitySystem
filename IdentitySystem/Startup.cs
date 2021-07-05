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
            // istemiþ olduðu sýnýfýn bir nesne örneðini oluþturur.
            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionString"]);

            });

          


            //IdentityUseri' App user olarak miras aldýk.
            // IdentityRole ile miras alma iþlemi gerçekleþtirmediðimizden kullanýyoruz.

            // ok iþareti çýkarttýðým zaman
            // bana gidip o ctordan istemiþ olduðu classtan bir tane class oluþturuyor.
            services.AddIdentity<AppUser, AppRole>(opts => {

                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters
                = "abcçdefghýijklmnoöpqrsþtuüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

                opts.Password.RequiredLength = 4; // default olarak en az 4 karakter isticez.
                opts.Password.RequireNonAlphanumeric = false; // yýldýz ya da nokta gibi karakter istemiyor
                opts.Password.RequireLowercase = false; // küçük harf istemiyorum
                opts.Password.RequireUppercase = false; // büyük harf istemiyorum.
                opts.Password.RequireDigit = false;// sayýsal karakter de istemiyorum 
            
            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "MyBlog";
            // kötü niyetli kullanýcýlar client side tarafta benim cookime eriþemez.
            // http isteði üzerinden cookie bilgisini almak istiyorum.
            cookieBuilder.HttpOnly = false;
            // süre belirtelim. ne kadar süre kullanýcýnýn bilgisayarýnda kalsýn
            // cookie 60 gün boyunca kalacak, login olduktan sonra 60 gün gezinebilecek. sonra tekrar login olamsý lazým
            // cookieBuilder.Expiration = System.TimeSpan.FromDays(60);
            // sadece benim sitem üzerinden gelen cookie ayarlarýný al
            cookieBuilder.SameSite = SameSiteMode.Lax;
            // always dersek browser sizin cookiesini , sadece bir https üzerinden bir istek gelmiþse gönderiyor.
            // SameAsRequest dersek, eðer bu cookie bilgisi http üzerinden gelmiþse http den gönderiyor
            // https derseniz htpps üzerinden gönderir
            // None dersek isterse https olsun ister http olsun hepsini http üzeirnden getiriyor.
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            services.ConfigureApplicationCookie(opts =>
            {
               
                // kullanýcý üye olmadan, üyelerin eriþebildiði bir sayfaya týklarsa kullanýcýyý login sayfasýna yönlendiririz.
                opts.LoginPath = new PathString("/Home/Login");
                opts.LogoutPath = new PathString("/Member/LogOut");

                opts.Cookie = cookieBuilder;

                // kullanýcýyýa 60 gün vermiþtik ya hani, eðer siz SlidingExpiration süresini true yaparsanýz.
                // 60'ýn yarýsýný geçtikten sonra eðer siteye istek yaparsa tekrar bi 60 gün daha eklicek.
                opts.SlidingExpiration = true;

                opts.ExpireTimeSpan = TimeSpan.FromDays(60);

                // eðer kullanýcý üye olduktan sonra, admin linkine týklarsa, editor rolüne sahip ama yönetici rolüne týklarsa 
                // bu sayfaya eriþemediðiyle ilgili bir bilgi verilir. Eriþime yetkisi olmayan üye kullanýcýlarýn gideceði path olacak
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied");

            });

            services.AddMvc(options => options.EnableEndpointRouting = false);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            app.UseDeveloperExceptionPage();// sayfamýzda bir hata aldýðýmýzda açýklayýcý bilgiler sunar
            app.UseStatusCodePages();// hatanýn nerde olduðunu gösteren bir yazý gösteriyor.
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
