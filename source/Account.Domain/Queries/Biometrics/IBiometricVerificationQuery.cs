using Account.Domain.Entities.Biometrics;
using System.Threading.Tasks;

namespace Account.Domain.Queries.Biometrics
{
    public interface IBiometricVerificationQuery
    {
        Task<BiometricVerification> GetByReference(string reference);
    }
}
