using Account.Domain.Enums;
using Account.Domain.Providers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Account.Data.Providers
{
    /// <summary>
    /// Provedor fake que simula mudança de status ao longo do tempo
    /// Útil para testar a Etapa 2 - verificações pendentes que depois ficam prontas
    /// </summary>
    public class StatefulFakeBiometricProvider : IBiometricProvider
    {
        private static readonly ConcurrentDictionary<string, (DateTime ApprovalTime, string FinalStatus)> _statusTimeline =
            new ConcurrentDictionary<string, (DateTime, string)>();

        public Task<BiometricProviderResponse> GetByReference(string reference)
        {
            string status = DetermineStatus(reference);

            return Task.FromResult(new BiometricProviderResponse
            {
                Status = status
            });
        }

        private string DetermineStatus(string reference)
        {
            // Regra 1: Se contém "approved", sempre retorna APPROVED
            if (reference.Contains("approved", StringComparison.OrdinalIgnoreCase))
                return "APPROVED";

            // Regra 2: Se contém "rejected", sempre retorna REJECTED
            if (reference.Contains("rejected", StringComparison.OrdinalIgnoreCase))
                return "REJECTED";

            // Regra 3: Simulação temporal
            // Primeira consulta: agenda aprovação para 5 segundos depois
            if (_statusTimeline.TryGetValue(reference, out var timeline))
            {
                if (DateTime.UtcNow >= timeline.ApprovalTime)
                    return timeline.FinalStatus; // APPROVED após 5 segundos
            }
            else
            {
                // Primeira vez: marcar que será aprovado em 5 segundos
                _statusTimeline.TryAdd(reference, (DateTime.UtcNow.AddSeconds(5), "APPROVED"));
            }

            return "PENDING";
        }

        /// <summary>
        /// Limpar estado entre testes
        /// </summary>
        public static void Reset()
        {
            _statusTimeline.Clear();
        }

        /// <summary>
        /// Para teste: marca uma referência como aprovada agora
        /// </summary>
        public static void ApproveNow(string reference)
        {
            _statusTimeline[reference] = (DateTime.UtcNow.AddSeconds(-1), "APPROVED");
        }
    }
}