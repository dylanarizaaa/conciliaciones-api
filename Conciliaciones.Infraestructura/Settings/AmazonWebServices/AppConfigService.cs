using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Conciliaciones.Dominio.Utils;
using Conciliaciones.Infraestructura.Ports.Interfaces.AWS;
using Microsoft.Extensions.Caching.Memory;

namespace Conciliaciones.Infraestructura.Settings.AmazonWebServices
{
    public class AppConfigService : IAppConfigServicio
    {
        private readonly IAmazonAppConfigData _client;
        private readonly IMemoryCache _cache;
        private readonly IParameterStoreService _parameterStore;
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        public AppConfigService(
            IAmazonAppConfigData client,
            IMemoryCache cache,
            IParameterStoreService iParameterStoreServicio)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _parameterStore = iParameterStoreServicio ?? throw new ArgumentNullException(nameof(iParameterStoreServicio));
        }

        public async Task<string> ObtenerConfiguracionAppConfig()
        {
            (string cacheKey, string tokenKey) = BuildCacheKeys();
            if (_cache.TryGetValue(cacheKey, out string? configCache))
            {
                return configCache!;
            }

            await Lock.WaitAsync();
            try
            {
                return await FetchWithRetryAsync(cacheKey, tokenKey);
            }
            catch
            {
                return StoreFallback(cacheKey, TimeSpan.FromMinutes(5));
            }
            finally
            {
                Lock.Release();
            }
        }

        private static (string CacheKey, string TokenKey) BuildCacheKeys()
        {
            string applicationId = Utilities.ObtenerVariableEntorno("applicationId");
            string environmentId = Utilities.ObtenerVariableEntorno("environmentId");
            string profileId = Utilities.ObtenerVariableEntorno("profileId");
            string cacheKey = $"AppConfig_{applicationId}_{environmentId}_{profileId}";
            return (cacheKey, $"{cacheKey}_token");
        }

        private async Task<string> FetchWithRetryAsync(string cacheKey, string tokenKey)
        {
            for (int intento = 1; intento <= 2; intento++)
            {
                try
                {
                    return await FetchAndCacheAsync(cacheKey, tokenKey);
                }
                catch (BadRequestException)
                {
                    if (intento == 1)
                    {
                        await RenewTokenAsync(tokenKey);
                        continue;
                    }
                }
                catch
                {
                    break;
                }
            }

            return ResolveFallback(cacheKey);
        }

        private async Task<string> FetchAndCacheAsync(string cacheKey, string tokenKey)
        {
            string token = await ResolveTokenAsync(tokenKey);
            GetLatestConfigurationResponse response = await _client.GetLatestConfigurationAsync(
                new GetLatestConfigurationRequest { ConfigurationToken = token });

            if (!string.IsNullOrEmpty(response.NextPollConfigurationToken))
            {
                _cache.Set(tokenKey, response.NextPollConfigurationToken);
            }

            string configJson = await ReadConfigurationAsync(response);
            TimeSpan cacheTime = TimeSpan.FromHours(await ResolveCacheHoursAsync());
            _cache.Set(cacheKey, configJson, cacheTime);
            return configJson;
        }

        private async Task<string> ResolveTokenAsync(string tokenKey)
        {
            if (_cache.TryGetValue(tokenKey, out string? token) && !string.IsNullOrEmpty(token))
            {
                return token;
            }

            return await IniciarNuevaSesion(
                Utilities.ObtenerVariableEntorno("applicationId"),
                Utilities.ObtenerVariableEntorno("environmentId"),
                Utilities.ObtenerVariableEntorno("profileId"));
        }

        private async Task RenewTokenAsync(string tokenKey)
        {
            string nuevoToken = await IniciarNuevaSesion(
                Utilities.ObtenerVariableEntorno("applicationId"),
                Utilities.ObtenerVariableEntorno("environmentId"),
                Utilities.ObtenerVariableEntorno("profileId"));
            _cache.Set(tokenKey, nuevoToken);
        }

        private static async Task<string> ReadConfigurationAsync(GetLatestConfigurationResponse response)
        {
            if (response.Configuration == null)
            {
                return "{}";
            }

            using StreamReader reader = new StreamReader(response.Configuration);
            string contenido = await reader.ReadToEndAsync();
            return string.IsNullOrWhiteSpace(contenido) ? "{}" : contenido;
        }

        private async Task<int> ResolveCacheHoursAsync()
        {
            try
            {
                string valorHoras = await _parameterStore.GetValueParameterStore("AppConfigCacheHours");
                return int.TryParse(valorHoras, out int horasCache) ? horasCache : 1;
            }
            catch
            {
                return 1;
            }
        }

        private string ResolveFallback(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out string? cachedFallback))
            {
                return cachedFallback!;
            }

            return StoreFallback(cacheKey, TimeSpan.FromMinutes(5));
        }

        private string StoreFallback(string cacheKey, TimeSpan ttl)
        {
            const string fallbackConfig = "{}";
            _cache.Set(cacheKey, fallbackConfig, ttl);
            return fallbackConfig;
        }

        private async Task<string> IniciarNuevaSesion(string applicationId, string environmentId, string profileId)
        {
            StartConfigurationSessionResponse response = await _client.StartConfigurationSessionAsync(
                new StartConfigurationSessionRequest
                {
                    ApplicationIdentifier = applicationId,
                    EnvironmentIdentifier = environmentId,
                    ConfigurationProfileIdentifier = profileId
                });

            return response.InitialConfigurationToken;
        }
    }
}
