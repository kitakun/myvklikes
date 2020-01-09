namespace Kitakun.VkModules.Web
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    using Autofac.Extensions.DependencyInjection;
#if RELEASE
    using Kitakun.VkModules.Persistance;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
#endif

    public class Program
    {
        public const int HttpPort = 5000;
        public const int HttpsPort = 5001;

        public static void Main(string[] args)
        {
            var buildedApp = CreateWebHostBuilder(args).Build();

#if RELEASE
            using (var scope = buildedApp.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<VkDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
#endif

            buildedApp.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var curDir = Directory.GetCurrentDirectory();
            Console.WriteLine($"AppDir: {curDir}");

            return WebHost
                .CreateDefaultBuilder(args)
#if RELEASE
                .UseKestrel(c =>
                {
                    c.ListenAnyIP(HttpPort);
                    c.ListenAnyIP(HttpsPort, cc =>
                    {
                        cc.UseHttps("myvklikes.pfx", "keysecret");
                    });
                })
#endif
                .UseContentRoot(curDir)
                .UseWebRoot(curDir)
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(urls: new string[] { $"http://*:{HttpPort}", $"https://*:{HttpsPort}" })
#if DEBUG
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
#endif
                .UseStartup<Startup>();
        }
    }
}
