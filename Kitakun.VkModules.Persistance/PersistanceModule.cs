namespace Kitakun.VkModules.Persistance
{
    using Autofac;

    public class PersistanceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<VkDbContext>()
                .AsSelf()
                .As<IVkDbContext>()
                .InstancePerLifetimeScope();
        }
    }
}
