using Account.Data.Queries;
using Account.Data.Stores;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;

namespace Account.Tests
{
    public class BiometricVerificationQueryTests
    {
        [Fact]
        public async Task GetByReference_WithValidReference_ShouldReturnVerification()
        {
            // Arrange
            var store = new BiometricVerificationStore();
            var verification = BiometricVerification.New();
            verification.Reference = "test-ref-1";
            verification.Status = BiometricVerificationStatus.Approved;
            verification.ResultDescription = "Verificação de biometria aprovada";
            
            store.Add(verification);

            var query = new BiometricVerificationQuery(store);

            // Act
            var result = await query.GetByReference("test-ref-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-ref-1", result.Reference);
            Assert.Equal(BiometricVerificationStatus.Approved, result.Status);
        }

        [Fact]
        public async Task GetByReference_WithNonExistentReference_ShouldReturnNull()
        {
            // Arrange
            var store = new BiometricVerificationStore();
            var query = new BiometricVerificationQuery(store);

            // Act
            var result = await query.GetByReference("non-existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPending_ShouldReturnOnlyPendingVerifications()
        {
            // Arrange
            var store = new BiometricVerificationStore();
            
            var approved = BiometricVerification.New();
            approved.Reference = "approved-1";
            approved.Status = BiometricVerificationStatus.Approved;
            
            var rejected = BiometricVerification.New();
            rejected.Reference = "rejected-1";
            rejected.Status = BiometricVerificationStatus.Rejected;
            
            var pending1 = BiometricVerification.New();
            pending1.Reference = "pending-1";
            pending1.Status = BiometricVerificationStatus.Pending;
            
            var pending2 = BiometricVerification.New();
            pending2.Reference = "pending-2";
            pending2.Status = BiometricVerificationStatus.Pending;

            store.Add(approved);
            store.Add(rejected);
            store.Add(pending1);
            store.Add(pending2);

            var query = new BiometricVerificationQuery(store);

            // Act
            var result = await query.GetAllPending();

            // Assert
            Assert.NotNull(result);
            var pendingList = result.ToList();
            Assert.Equal(2, pendingList.Count);
            Assert.All(pendingList, v => Assert.Equal(BiometricVerificationStatus.Pending, v.Status));
        }

        [Fact]
        public async Task GetAllPending_WithNoPendingVerifications_ShouldReturnEmptyList()
        {
            // Arrange
            var store = new BiometricVerificationStore();
            
            var approved = BiometricVerification.New();
            approved.Reference = "approved-1";
            approved.Status = BiometricVerificationStatus.Approved;
            
            var rejected = BiometricVerification.New();
            rejected.Reference = "rejected-1";
            rejected.Status = BiometricVerificationStatus.Rejected;

            store.Add(approved);
            store.Add(rejected);

            var query = new BiometricVerificationQuery(store);

            // Act
            var result = await query.GetAllPending();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}