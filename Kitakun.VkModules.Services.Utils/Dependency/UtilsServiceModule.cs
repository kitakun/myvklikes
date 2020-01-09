namespace Kitakun.VkModules.Services.Utils.Dependency
{
    using Autofac;

    using Kitakun.VkModules.Services.Abstractions;

    public sealed class UtilsServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SessionSecretGenerator>()
                .As<ISessionSecretGenerator>()
                .InstancePerLifetimeScope();
        }
    }
}