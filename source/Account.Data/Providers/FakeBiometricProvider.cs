using Account.Domain.Providers;
using System.Threading.Tasks;

namespace Account.Data.Providers
{
    public class FakeBiometricProvider : IBiometricProvider
    {
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
            if (reference.Contains("approved", System.StringComparison.OrdinalIgnoreCase))
                return "APPROVED";
            
            if (reference.Contains("rejected", System.StringComparison.OrdinalIgnoreCase))
                return "REJECTED";
            
            return "PENDING";
        }
    }
}
