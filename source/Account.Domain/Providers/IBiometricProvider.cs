using System.Threading.Tasks;

namespace Account.Domain.Providers
{
    public interface IBiometricProvider
    {
        Task<BiometricProviderResponse> GetByReference(string reference);
    }
}
