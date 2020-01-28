﻿namespace Kitakun.VkModules.Services.Abstractions
{
    public interface IWebContext
    {
        bool IsAdmin { get; }

        bool IsUltraAdmin { get; }

        long GroupId { get; }

        string VkAccessToken { get; }
    }
}
