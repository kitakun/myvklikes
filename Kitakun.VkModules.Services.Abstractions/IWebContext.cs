namespace Kitakun.VkModules.Services.Abstractions
{
    public interface IWebContext
    {
        bool IsAdmin { get; }

        bool IsUltraAdmin { get; }

        long GroupId { get; }

        /// <summary>
        /// Is md5 of auth_key correct (anti-hack protection)
        /// </summary>
        bool IsVkFrameValid { get; }
    }
}
