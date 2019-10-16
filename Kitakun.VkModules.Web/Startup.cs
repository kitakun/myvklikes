namespace Kitakun.VkModules.Web
{
    using System;

	using Autofac;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;

	using Kitakun.VkModules.Web.Infrastructure;

    public class Startup
	{
		public IConfiguration Configuration { get; }

		public IServiceCollection CoreServices { get; private set; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
			IApplicationLifetime appLifetime)
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

			//app.UseAuthentication();
			app.UseStaticFiles();
			
			//app.UseSpaStaticFiles();

			app.UseMvcWithDefaultRoute();

			//app.UseSpa(spa =>
			//{
			//	spa.Options.SourcePath = "ClientApp/PlumsailAppClient";

			//	if (env.IsDevelopment())
			//	{
			//		//spa.UseAngularCliServer(npmScript: "serve");
			//	}
			//});
		}
	}
}
