namespace Kitakun.VkModules.Web.Infrastructure
{
	using Autofac;

	using Kitakun.VkModules.Persistance;
	using Kitakun.VkModules.Services.GroupLikeService.Dependency;
	using Kitakun.VkModules.Services.PersistanceServices.Dependency;
    using Kitakun.VkModules.Services.Utils.Dependency;

    public static class DependencyConfiguration
	{
		public static void Configurate(this ContainerBuilder container)
		{
			container.RegisterModule<PersistanceModule>();
			container.RegisterModule<GroupLikeServiceModule>();
			container.RegisterModule<PersistanceServicesModule>();
            container.RegisterModule<UtilsServiceModule>();
            container.RegisterModule<WebModule>();
		}
	}
}
