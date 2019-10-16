namespace Kitakun.VkModules.Web.Infrastructure
{
	using Autofac;

	using Kitakun.VkModules.Services.GroupLikeService.Dependency;

	public static class DependencyConfiguration
	{
		public static void Configurate(this ContainerBuilder container)
		{
			container.RegisterModule<GroupLikeServiceModule>();
			container.RegisterModule<WebModule>();
		}
	}
}
