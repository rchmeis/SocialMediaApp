using SocialMediaAppLibrary.Services;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Database;
using Microsoft.AspNetCore.Authentication;

namespace SocialMediaApp
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<DbConnect>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IPrivateMessageService, PrivateMessageService>();
            builder.Services.AddScoped<IPrivateMessageRepository, PrivateMessageRepository>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
            builder.Services.AddScoped<IPartnerService, PartnerService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IUserAuthenticationRepository, UserAuthenticationRepository>();

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

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
