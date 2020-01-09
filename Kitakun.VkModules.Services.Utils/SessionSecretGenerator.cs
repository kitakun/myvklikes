namespace Kitakun.VkModules.Services.Utils
{
    using System;
    using System.Text;

    using Kitakun.VkModules.Services.Abstractions;

    public sealed class SessionSecretGenerator : ISessionSecretGenerator
    {
        public string GetSessionSecret()
        {
            var currentTime = DateTime.Now;

            var timesecret = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 1, 1);

            var generated = $"ra{timesecret}se{timesecret}";
            var bytes = Encoding.UTF8.GetBytes(generated);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                stringBuilder.Append(bytes[i]);
            }

            return stringBuilder.ToString();
        }
    }
}
