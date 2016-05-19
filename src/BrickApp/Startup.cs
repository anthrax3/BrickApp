﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BrickApp.Models.Identity;
using BrickApp.Services;
using BrickApp.Models.BrickStore;
using BrickApp.Logging;
using BrickApp.Models;
using BrickApp.Models.Bricks;

namespace BrickApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("secrets.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build().ReloadOnChanged("appsettings.json");
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<BrickContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:BrickApp"]))
                .AddDbContext<BrickStoreContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:BrickApp"]))
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:BrickApp"]));

            services.AddSingleton<CategoryCache>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddProvider(new SqlLoggerProvider());
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                app.UseStatusCodePages();
                app.UseStatusCodePagesWithRedirects("/Error");

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<BrickStoreContext>().Database.Migrate();
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
                }

                app.EnsureSampleData();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();
            app.EnsureRolesCreated();

            // See comments in config.json for info on enabling Facebook auth
            var facebookId = Configuration["Auth:Facebook:AppId"];
            var facebookSecret = Configuration["Auth:Facebook:AppSecret"];
            if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
            {
                app.UseFacebookAuthentication(options =>
                {
                    options.AppId = facebookId;
                    options.AppSecret = facebookSecret;
                });
            }

            // See comments in config.json for info on enabling Google auth
            var googleId = Configuration["Auth:Google:ClientId"];
            var googleSecret = Configuration["Auth:Google:ClientSecret"];
            if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
            {
                app.UseGoogleAuthentication(options =>
                {
                    options.ClientId = googleId;
                    options.ClientSecret = googleSecret;
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
