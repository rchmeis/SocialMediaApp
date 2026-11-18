using SocialMediaAppLibrary.Services;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SocialMediaApp
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            builder.Services.AddSession();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    //options.AccessDeniedPath = "/Account/AccessDenied";   //Not necessary because we're doing a redirect with appended query parameter
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;

                    options.Cookie.HttpOnly = true;                         // Mitigate XSS attacks (JS cants read or write to the cookie)
                    options.Cookie.SameSite = SameSiteMode.Lax;             // protects against CSRF attacks (cookie is only sent for same-site requests)
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   ensures the cookie is only sent over encrypted HTTPS connections. (uncomment this line when deploying to production)

                    options.Events.OnRedirectToAccessDenied = context =>    //redirects the user and appends a query parameter. The UI  can then display an error message upon redirect without disrupting the user by moving them to an error page.
                    {
                        context.Response.Redirect("/Account/Profile/Index?accessDenied=true");
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorization(options=>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole(Role.Admin.ToString()));
                options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole(Role.Moderator.ToString()));
                options.AddPolicy("StudentPolicy", policy => policy.RequireRole(Role.Student.ToString()));
                options.AddPolicy("PartnerPolicy", policy => policy.RequireRole(Role.Partner.ToString()));
            });

            builder.Services.AddRazorPages(options =>
            {                
                options.Conventions.AuthorizeFolder("/");                  //everything requires login

                options.Conventions.AllowAnonymousToPage("/Index");        //except for..
                options.Conventions.AllowAnonymousToPage("/Account/SignIn");
                options.Conventions.AllowAnonymousToPage("/Account/SignUp");
                //We will add policies directly to the relevant razor pages using eg: [Authorize(Policy = "PartnerPolicy")]

            });

            builder.Services.AddScoped<IDbConnect, DbConnect>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
            builder.Services.AddScoped<IPartnerService, PartnerService>();
            builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
            builder.Services.AddScoped<IUserAuthenticationRepository, UserAuthenticationRepository>();
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
