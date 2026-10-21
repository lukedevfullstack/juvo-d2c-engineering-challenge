using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using System;

namespace Account.Domain.Aggregates.Biometrics
{
    public class BiometricVerificationAggregate
    {
        public void ApplyResult(BiometricVerification verification, BiometricVerificationStatus status, string resultDescription)
        {
            verification.Status = status;
            verification.ResultDescription = resultDescription;
            verification.UpdatedAt = DateTime.UtcNow;
        }
    }
}
