using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Domain.Entity;

namespace Account.Domain.Entities.Biometrics
{
    public class BiometricVerification : Entity<BiometricVerification>
    {
        public string Reference { get; set; }
        public BiometricVerificationStatus Status { get; set; }
        public string ResultDescription { get; set; }
    }
}
