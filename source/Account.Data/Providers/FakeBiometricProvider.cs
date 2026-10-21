using Account.Domain.Providers;
using System;
using System.Threading.Tasks;

namespace Account.Data.Providers
{
    public class FakeBiometricProvider : IBiometricProvider
    {
        public Task<BiometricProviderResponse> GetByReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Reference is required.", nameof(reference));

            var normalized = reference.ToLowerInvariant();

            if (normalized.Contains("timeout"))
                throw new TimeoutException("The biometric provider did not respond in time.");

            if (normalized.Contains("approved"))
                return Task.FromResult(new BiometricProviderResponse { Status = "APPROVED" });

            if (normalized.Contains("rejected"))
                return Task.FromResult(new BiometricProviderResponse { Status = "REJECTED" });

            if (normalized.Contains("pending"))
                return Task.FromResult(new BiometricProviderResponse { Status = "PENDING" });

            return Task.FromResult(new BiometricProviderResponse { Status = "PENDING" });
        }
    }
}
