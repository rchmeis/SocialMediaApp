using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);           

            // Add services to the container.

            // Session
            builder.Services.AddDistributedMemoryCache(); // In-memory(ram) store for session data.
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".StudentHub.Session"; // session name
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session idle time before deleting
                options.Cookie.HttpOnly = true; // cookie can't be read by JavaScript
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true; // required for GDPR. Marks the cookie as strictly necessary
            });


            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/SignIn";
                    options.LogoutPath = "/Account/SignOut";
                    //options.AccessDeniedPath = "/Account/AccessDenied";   //Not necessary because we're doing a redirect with appended query parameter
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;

                    options.Cookie.HttpOnly = true;                         // Mitigate XSS attacks (JS cants read or write to the cookie)
                    options.Cookie.SameSite = SameSiteMode.Lax;             // protects against CSRF attacks (cookie is only sent for same-site requests)
                    
                    // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  //Cookie is only sent over encrypted HTTPS connections.

                    // Delete 3 lines of code below once in production.
                    // This code ensures that we can test cookies in HTTP while in development
                    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                        ? CookieSecurePolicy.SameAsRequest   // http is fine while developing
                        : CookieSecurePolicy.Always;         // HTTPS-only once deployed
                    
                    
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
                options.Conventions.AuthorizeFolder("/Account/OnboardingFlow", "StudentPolicy"); // Only logged in students can access


                options.Conventions.AllowAnonymousToPage("/Index");        //except for..
                options.Conventions.AllowAnonymousToPage("/Account/SignIn");
                options.Conventions.AllowAnonymousToPage("/Account/SignupFlow/Step1");
                options.Conventions.AllowAnonymousToPage("/Account/SignupFlow/Step2");
                options.Conventions.AllowAnonymousToPage("/ImageUploadTest"); //! Temporary page for testing image upload functionality. Remove this line when the page is no longer needed.
                //We will add policies directly to the relevant razor pages using eg: [Authorize(Policy = "PartnerPolicy")]

            });

            builder.Services.AddMemoryCache();
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
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IImageRepository, ImageRepository>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IInstitutionRepository, InstitutionRepository>();
            builder.Services.AddScoped<IInstitutionService, InstitutionService>();
            builder.Services.AddScoped<IVerificationService, VerificationService>();
            builder.Services.AddScoped<IInterestService, InterestService>();
            builder.Services.AddScoped<IInterestRepository, InterestRepository>();
            builder.Services.AddScoped<ILikeService, LikeService>();
            builder.Services.AddScoped<ILikeRepository, LikeRepository>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Handles all errors. error code replaces the "{0}".


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
