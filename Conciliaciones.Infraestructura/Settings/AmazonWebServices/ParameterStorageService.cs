using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Conciliaciones.Infraestructura.Ports.Interfaces.AWS;
using Microsoft.Extensions.Caching.Memory;

namespace Conciliaciones.Infraestructura.Settings.AmazonWebServices
{
    public class ParameterStorageService : IParameterStoreService
    {
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly IMemoryCache _cache;

        public ParameterStorageService(IAmazonSimpleSystemsManagement ssmClient, IMemoryCache cache)
        {
            _ssmClient = ssmClient ?? throw new ArgumentNullException(nameof(ssmClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public Task<string> GetValueParameterStore(string nombreParametro)
        {
            return GetValueParameterStore(nombreParametro, CancellationToken.None);
        }

        public async Task<string> GetValueParameterStore(string nombreParametro, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
            {
                throw new ArgumentNullException(nameof(nombreParametro), "El nombre del parámetro no puede ser nulo ni estar vacío.");
            }

            try
            {
                GetParameterRequest request = new GetParameterRequest
                {
                    Name = nombreParametro,
                    WithDecryption = true
                };

                GetParameterResponse response = await _ssmClient.GetParameterAsync(request, cancellationToken);
                return response.Parameter.Value;
            } catch (Exception ex)
            {
                throw new Exception($"Error al obtener el valor del parámetro '{nombreParametro}' desde Parameter Store: {ex.Message}", ex);
            }
        }
    }
}
