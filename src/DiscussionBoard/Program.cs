using DiscussionBoard.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Serilog.Formatting.Compact;

namespace DiscussionBoard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Get the location of the Logs folder for SeriLog
            string logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs",  "discussionboard-.json");

            builder.Services.AddSerilog((services, configuration) => configuration
                .WriteTo.Console()
                .WriteTo.File(
                    new RenderedCompactJsonFormatter(),
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    shared: true)
            );

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                   throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment())
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Forums");
                options.Conventions.AuthorizeFolder("/Discussions");
                options.Conventions.AuthorizePage("/MyProfile");
            });

            builder.Services.AddSingleton<ILogReader, JsonLogReader>();

            builder.Services.AddScoped<DiscussionThreadService>();

            builder.Services.AddScoped<PostService>();

            builder.Services.AddScoped<ForumService>();

            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            builder.Services.AddScoped<IApplicationAuthorizationService, ApplicationAuthorizationService>();

            // Use the resource-based policy to authorize edits to threads and individual posts.
            builder.Services.AddAuthorizationBuilder()
                            .AddPolicy(ApplicationAuthorizationPolicies.IsOwnerOrAdmin, policy =>
                {
                    policy.Requirements.Add(new IsOwnerOrAdminRequirement());
                });

            // The handler handles the policy by checking administrator status and author ownership.
            builder.Services.AddSingleton<IAuthorizationHandler, IsOwnerOrAdminHandler>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
                .WithStaticAssets();

            app.Run();
        }
    }
}
