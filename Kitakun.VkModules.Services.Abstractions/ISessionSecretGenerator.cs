namespace Kitakun.VkModules.Services.Abstractions
{
    public interface ISessionSecretGenerator
    {
        /// <summary>
        /// Create timestamp secret for admin area access
        /// </summary>
        string GetSessionSecret();
    }
}
