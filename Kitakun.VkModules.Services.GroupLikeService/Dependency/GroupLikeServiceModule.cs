namespace Kitakun.VkModules.Services.GroupLikeService.Dependency
{
	using Autofac;

	using Kitakun.VkModules.Services.Abstractions;

	public sealed class GroupLikeServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<GroupLikesService>()
				.As<IGroupLikesService>()
				.InstancePerLifetimeScope();

            builder
                .RegisterType<Top100Service>()
                .As<ITop100Service>()
                .InstancePerLifetimeScope();
        }
	}
}
