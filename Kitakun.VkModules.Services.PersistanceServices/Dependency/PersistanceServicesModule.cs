namespace Kitakun.VkModules.Services.PersistanceServices.Dependency
{
    using Autofac;

    using Kitakun.VkModules.Services.Abstractions;

    public sealed class PersistanceServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DataCollectionsService>()
                .As<IDataCollectionsService>()
                .InstancePerLifetimeScope();
        }
    }
}
