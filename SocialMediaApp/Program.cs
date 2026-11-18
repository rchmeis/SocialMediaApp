using SocialMediaAppLibrary.Services;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Session
            builder.Services.AddDistributedMemoryCache(); // In-memory(ram) store for session data.
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".StudentHub.Session"; // session name
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session idle time before deleting
                options.Cookie.HttpOnly = true; // cookie can't be read by JavaScript
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Cookies are only sent over HTTPS (not HTTP!)
                options.Cookie.IsEssential = true; // required for GDPR. Marks the cookie as strictly necessary
            });

            // Add services to the container.
            builder.Services.AddSession();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            builder.Services.AddRazorPages();
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
