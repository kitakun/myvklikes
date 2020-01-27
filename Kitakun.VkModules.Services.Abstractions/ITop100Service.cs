namespace Kitakun.VkModules.Services.Abstractions
{
    using System.Threading.Tasks;

    using Kitakun.VkModules.Core.Models;

    public interface ITop100Service
    {
        Task<Top100BestLikersModel> LoadTop100(long groupId, bool forceRecalc);
    }
}
