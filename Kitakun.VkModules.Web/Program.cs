namespace Kitakun.VkModules.Web
{
	using System.IO;

	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;

	using Autofac.Extensions.DependencyInjection;

	public class Program
	{
		public const int HttpPort = 5000;
		public const int HttpsPort = 5001;

		public static void Main(string[] args) =>
			CreateWebHostBuilder(args)
				.Build()
				.Run();

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost
				.CreateDefaultBuilder(args)
				.ConfigureServices(services => services.AddAutofac())
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseWebRoot(Directory.GetCurrentDirectory())
				.UseUrls(urls: new string[] { $"http://*:{HttpPort}", $"https://*:{HttpsPort}" })
				.UseStartup<Startup>();
	}
}
