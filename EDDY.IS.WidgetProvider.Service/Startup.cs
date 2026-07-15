using EDDY.IS.WidgetProvider.Core;
using EDDY.IS.WidgetProvider.Core.Interfaces;
using EDDY.IS.WidgetProvider.Core.Services;
using EDDY.IS.WidgetProvider.Data;
using EDDY.IS.WidgetProvider.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;

namespace EDDY.IS.WidgetProvider.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            string trackingConnectionString = Configuration.GetConnectionString("EddyTrackingISConnection");

            services.AddDbContext<NexusContext>((options) => options.UseSqlServer(connectionString));
            services.AddDbContext<EddyTrackingISContext>((options) => options.UseSqlServer(trackingConnectionString));

            //services.AddHttpClient<IGPListingApiService, GPListingApiService>();
            services.AddHttpClient();
            services.AddTransient<IGPListingApiService,GPListingApiService>();

            services.AddSingleton<IFESessionRedisService, FESessionRedisService>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddMemoryCache();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            ConfigureDependencies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ICacheService cache, IWidgetRepository widgetRepository)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Caching Url Configurations in memory for fast retrieval
            if (widgetRepository != null)
            {
                Dictionary<string, Dictionary<string, string>> urlConfigs = widgetRepository.GetVendorUrlConfigurations();

                if (urlConfigs != null)
                    cache.SetCacheItem("URLCONFIGS", urlConfigs, indefiniteExpiration: true);
            }

            //app.UseHttpsRedirection();
            //app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "testclients")),
                RequestPath = "/testclients"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "css")),
                RequestPath = "/css"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "images")),
                RequestPath = "/images"
            });

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ConfigureDependencies(IServiceCollection services)
        {
            services.AddScoped<IWidgetPackageService, WidgetPackageService>();
            services.AddScoped<IFileSerializeService, FileSerializeService>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddScoped<IWidgetRepository, WidgetRepository>();
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<IMinificationService, MinificationService>();
            services.AddScoped<IQDFService, QDFService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IModelInstantiationService, ModelInstantiationService>();
        }
    }
}
