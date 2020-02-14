namespace Kitakun.VkModules.Web
{
#if RELEASE
    using System;
    using Microsoft.AspNetCore.Http;
#endif
    using Autofac;
    using Hangfire;
    using Hangfire.PostgreSql;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Kitakun.VkModules.Web.Infrastructure;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IServiceCollection CoreServices { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(x => x.SslPort = Program.HttpsPort)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(c =>
            {
                c.AddPolicy(WebConstants.AllCorsName, options => options.AllowAnyOrigin());
            });

            services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);

            services.AddHangfire(x => x.UsePostgreSqlStorage(Configuration.GetConnectionString("HangfireConnection")));
            services.AddHangfireServer();
#if RELEASE
			services.AddHsts(options =>
			{
				options.Preload = true;
				options.IncludeSubDomains = true;
				options.MaxAge = TimeSpan.FromDays(60);
			});

			services.AddHttpsRedirection(options =>
			{
				options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
				options.HttpsPort = Program.HttpsPort;
			});
#endif
        }

        public void ConfigureContainer(ContainerBuilder builder) => builder.Configurate();

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime appLifetime,
            ILoggerFactory loggerFactory)
        {
            app.UseMiddleware<ApiErrorHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHangfireDashboard(pathMatch: "/secrethangfire", new DashboardOptions
            {
                IgnoreAntiforgeryToken = true,
                Authorization = new[] { new HangFireAuthorizationFilter(Configuration) }
            });

            //app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "MyArea",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                   name: "default",
                   template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
