using Account.Domain.Aggregates.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Account.Tasks.Jobs
{
    public class SyncBiometricVerificationsJob : IScheduledJob
    {
        private readonly IServiceProvider _serviceProvider;

        public SyncBiometricVerificationsJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string Schedule => "0 */15 * * * *";

        public async Task DoWorkAsync()
        {
            // Criar um scope para resolver serviços scoped
            using var scope = _serviceProvider.CreateScope();
            var query = scope.ServiceProvider.GetRequiredService<IBiometricVerificationQuery>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            var provider = scope.ServiceProvider.GetRequiredService<IBiometricProvider>();

            // Busca todas as verificações pendentes
            var pendingVerifications = await query.GetAllPending();
            
            var aggregate = new BiometricVerificationAggregate();

            foreach (var verification in pendingVerifications)
            {
                try
                {
                    // Consulta o provedor novamente
                    var providerResponse = await provider.GetByReference(verification.Reference);
                    var newStatus = ConvertProviderStatusToDomainStatus(providerResponse.Status);

                    // Se o status mudou de PENDING para um estado final, atualiza
                    if (newStatus != BiometricVerificationStatus.Pending)
                    {
                        verification.Status = newStatus;
                        verification.ResultDescription = GetResultDescription(newStatus);
                        verification.UpdatedAt = DateTime.UtcNow;

                        await repository.UpdateAsync(verification);
                    }
                }
                catch (Exception)
                {
                    // Log do erro e continua com a próxima verificação
                    continue;
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

        private string GetResultDescription(BiometricVerificationStatus status)
        {
            return status switch
            {
                BiometricVerificationStatus.Approved => "Verificação de biometria aprovada",
                BiometricVerificationStatus.Rejected => "Verificação de biometria reprovada",
                BiometricVerificationStatus.Pending => "Verificação de biometria em processamento",
                _ => null
            };
        }
    }
}
