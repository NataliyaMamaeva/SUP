using ERP.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Services
{
    public class YandexTokenRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<YandexTokenRefreshService> _logger;
        private readonly IDataProtector _protector;
        private readonly YandexDiskSettings _yandexSettings;

        public YandexTokenRefreshService(IServiceProvider serviceProvider, IOptions<YandexDiskSettings> yandexSettings, ILogger<YandexTokenRefreshService> logger, IDataProtectionProvider provider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _protector = provider.CreateProtector("YandexTokenProtector");
            _yandexSettings = yandexSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("⏳ Сервис обновления токенов Яндекса запущен.");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ErpContext>();

                    var expiringAccounts = await context.YandexAccounts
                        .Where(a => a.ExpiryDate <= DateTime.UtcNow.AddDays(7))
                        .ToListAsync(stoppingToken);

                    foreach (var account in expiringAccounts)
                    {
                        await RefreshTokenAsync(account, context);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении токенов Яндекса.");
                }
                _logger.LogInformation("⏳ Ожидание следующей проверки (1 день)...");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task RefreshTokenAsync(YandexAccount account, ErpContext context)
        {
            using var client = new HttpClient();
            var refreshValues = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", account.RefreshToken },
                { "client_id", "ВАШ_CLIENT_ID" },
                { "client_secret", "ВАШ_CLIENT_SECRET" }
            };

            var refreshContent = new FormUrlEncodedContent(refreshValues);
            var refreshResponse = await client.PostAsync("https://oauth.yandex.ru/token", refreshContent);
            var refreshResponseString = await refreshResponse.Content.ReadAsStringAsync();

            var refreshJson = JObject.Parse(refreshResponseString);
            var newAccessToken = refreshJson["access_token"]?.ToString();
            var newRefreshToken = refreshJson["refresh_token"]?.ToString();

            if (!string.IsNullOrEmpty(newAccessToken))
            {
                account.AccessToken = newAccessToken;
                account.RefreshToken = newRefreshToken ?? account.RefreshToken;
                account.ExpiryDate = DateTime.UtcNow.AddYears(1);

                await context.SaveChangesAsync();
                _logger.LogInformation($"Токен обновлён для аккаунта {account.Id}");
            }
            else
            {
                _logger.LogWarning($"Не удалось обновить токен для аккаунта {account.Id}: {refreshResponseString}");
            }
        }
    }
}
