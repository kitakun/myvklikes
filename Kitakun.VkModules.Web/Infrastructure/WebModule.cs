namespace Kitakun.VkModules.Web.Infrastructure
{
    using Autofac;

    using Microsoft.AspNetCore.Http;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.Services;
    using Kitakun.VkModules.Web.Controllers;

    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<WebContext>().As<IWebContext>().InstancePerLifetimeScope();
            builder.RegisterType<BackgroundUpdater>().AsSelf().InstancePerLifetimeScope();
            //builder.RegisterType<UserProvider>().As<IUserProvider>().InstancePerLifetimeScope();
        }
    }
}
