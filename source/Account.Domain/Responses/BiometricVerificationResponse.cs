using Account.Domain.Entities.Biometrics;

namespace Account.Domain.Responses
{
    public class BiometricVerificationResponse
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string ResultDescription { get; set; }
        public System.DateTime CreateDate { get; set; }
        public System.DateTime? UpdatedAt { get; set; }

        public static BiometricVerificationResponse From(BiometricVerification verification)
        {
            return new BiometricVerificationResponse
            {
                Id = verification.Id.ToString(),
                Reference = verification.Reference,
                Status = verification.Status.ToString(),
                ResultDescription = verification.ResultDescription,
                CreateDate = verification.CreateDate,
                UpdatedAt = verification.UpdatedAt
            };
        }
    }
}
