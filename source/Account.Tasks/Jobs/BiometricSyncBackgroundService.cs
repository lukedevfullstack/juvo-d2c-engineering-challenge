using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Tasks.Jobs
{
    public class BiometricSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BiometricSyncBackgroundService> _logger;

        public BiometricSyncBackgroundService(IServiceProvider serviceProvider, ILogger<BiometricSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de sincronização biométrica iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar a sincronização das verificações biométricas.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        public async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var query = scope.ServiceProvider.GetRequiredService<IBiometricVerificationQuery>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            var provider = scope.ServiceProvider.GetRequiredService<IBiometricProvider>();

            var pendingVerifications = await query.GetAllPending();

            foreach (var verification in pendingVerifications)
            {
                try
                {
                    var providerResult = await provider.GetByReference(verification.Reference);

                    if (providerResult.Status != verification.Status.ToString())
                    {
                        verification.UpdateStatus(ConvertProviderStatusToDomainStatus(providerResult.Status));
                        await repository.UpdateAsync(verification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar a referência {Reference}", verification.Reference);
                }
            }
        }

        private BiometricVerificationStatus ConvertProviderStatusToDomainStatus(string providerStatus)
        {
            return providerStatus switch
            {
                "APPROVED" => BiometricVerificationStatus.Approved,
                "REJECTED" => BiometricVerificationStatus.Rejected,
                "PENDING" => BiometricVerificationStatus.Pending,
                _ => BiometricVerificationStatus.Pending
            };
        }
    }
}