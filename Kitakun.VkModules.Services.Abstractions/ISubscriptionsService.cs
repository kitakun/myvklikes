namespace Kitakun.VkModules.Services.Abstractions
{
    using System.Threading.Tasks;

    public interface ISubscriptionsService
    {
        Task<bool> GroupHasSubscription(long groupId);
    }
}
