using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Tasks.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Account.Tests
{
    public class BiometricSyncJobTests
    {
        private IServiceProvider CreateServiceProvider(
            IBiometricVerificationQuery query,
            IRepository repository,
            IBiometricProvider provider)
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => query);
            services.AddScoped(_ => repository);
            services.AddScoped(_ => provider);
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task DoWorkAsync_WithPendingVerifications_ShouldUpdateToApprovedStatus()
        {
            // Arrange
            var pending = BiometricVerification.New();
            pending.Reference = "verification-pending-1";

            var mockQuery = new Mock<IBiometricVerificationQuery>();
            mockQuery.Setup(x => x.GetAllPending()).ReturnsAsync(new[] { pending });

            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<BiometricVerification>())).Returns(Task.CompletedTask);

            var mockProvider = new Mock<IBiometricProvider>();
            mockProvider.Setup(x => x.GetByReference("verification-pending-1"))
                        .ReturnsAsync(new BiometricProviderResponse { Status = "APPROVED" });

            var serviceProvider = CreateServiceProvider(mockQuery.Object, mockRepository.Object, mockProvider.Object);
            var job = new BiometricSyncBackgroundService(serviceProvider, Mock.Of<Microsoft.Extensions.Logging.ILogger<BiometricSyncBackgroundService>>());

            // Act
            await job.DoWorkAsync();

            // Assert
            Assert.Equal(BiometricVerificationStatus.Approved, pending.Status);
            mockRepository.Verify(x => x.UpdateAsync(pending), Times.Once);
        }

        [Fact]
        public async Task DoWorkAsync_WithStillPendingVerifications_ShouldNotUpdate()
        {
            // Arrange
            var pending = BiometricVerification.New();
            pending.Reference = "verification-pending-1";

            var mockQuery = new Mock<IBiometricVerificationQuery>();
            mockQuery.Setup(x => x.GetAllPending()).ReturnsAsync(new[] { pending });

            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<BiometricVerification>())).Returns(Task.CompletedTask);

            var mockProvider = new Mock<IBiometricProvider>();
            mockProvider.Setup(x => x.GetByReference("verification-pending-1"))
                        .ReturnsAsync(new BiometricProviderResponse { Status = "PENDING" });

            var serviceProvider = CreateServiceProvider(mockQuery.Object, mockRepository.Object, mockProvider.Object);
            var job = new BiometricSyncBackgroundService(serviceProvider, Mock.Of<Microsoft.Extensions.Logging.ILogger<BiometricSyncBackgroundService>>());

            // Act
            await job.DoWorkAsync();

            // Assert
            Assert.Equal(BiometricVerificationStatus.Pending, pending.Status);
            mockRepository.Verify(x => x.UpdateAsync(It.IsAny<BiometricVerification>()), Times.Once);
        }

        [Fact]
        public async Task DoWorkAsync_WithMultiplePendingVerifications_ShouldUpdateAll()
        {
            // Arrange
            var pending1 = BiometricVerification.New();
            pending1.Reference = "verification-pending-1";

            var pending2 = BiometricVerification.New();
            pending2.Reference = "verification-pending-2";

            var pending3 = BiometricVerification.New();
            pending3.Reference = "verification-pending-3";

            var mockQuery = new Mock<IBiometricVerificationQuery>();
            mockQuery.Setup(x => x.GetAllPending()).ReturnsAsync(new[] { pending1, pending2, pending3 });

            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<BiometricVerification>())).Returns(Task.CompletedTask);

            var mockProvider = new Mock<IBiometricProvider>();
            mockProvider.Setup(x => x.GetByReference("verification-pending-1")).ReturnsAsync(new BiometricProviderResponse { Status = "APPROVED" });
            mockProvider.Setup(x => x.GetByReference("verification-pending-2")).ReturnsAsync(new BiometricProviderResponse { Status = "REJECTED" });
            mockProvider.Setup(x => x.GetByReference("verification-pending-3")).ReturnsAsync(new BiometricProviderResponse { Status = "PENDING" });

            var serviceProvider = CreateServiceProvider(mockQuery.Object, mockRepository.Object, mockProvider.Object);
            var job = new BiometricSyncBackgroundService(serviceProvider, Mock.Of<Microsoft.Extensions.Logging.ILogger<BiometricSyncBackgroundService>>());

            // Act
            await job.DoWorkAsync();

            // Assert
            Assert.Equal(BiometricVerificationStatus.Approved, pending1.Status);
            Assert.Equal(BiometricVerificationStatus.Rejected, pending2.Status);
            Assert.Equal(BiometricVerificationStatus.Pending, pending3.Status);

            mockRepository.Verify(x => x.UpdateAsync(It.IsAny<BiometricVerification>()), Times.Exactly(3));
        }

        [Fact]
        public async Task DoWorkAsync_WithProviderException_ShouldContinueProcessing()
        {
            // Arrange
            var pending1 = BiometricVerification.New();
            pending1.Reference = "verification-pending-1";

            var pending2 = BiometricVerification.New();
            pending2.Reference = "verification-pending-2";

            var mockQuery = new Mock<IBiometricVerificationQuery>();
            mockQuery.Setup(x => x.GetAllPending()).ReturnsAsync(new[] { pending1, pending2 });

            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<BiometricVerification>())).Returns(Task.CompletedTask);

            var mockProvider = new Mock<IBiometricProvider>();
            mockProvider.Setup(x => x.GetByReference("verification-pending-1")).ThrowsAsync(new Exception("Provider error"));
            mockProvider.Setup(x => x.GetByReference("verification-pending-2")).ReturnsAsync(new BiometricProviderResponse { Status = "APPROVED" });

            var serviceProvider = CreateServiceProvider(mockQuery.Object, mockRepository.Object, mockProvider.Object);
            var job = new BiometricSyncBackgroundService(serviceProvider, Mock.Of<Microsoft.Extensions.Logging.ILogger<BiometricSyncBackgroundService>>());

            // Act
            await job.DoWorkAsync();

            // Assert
            Assert.Equal((BiometricVerificationStatus)0, pending1.Status);
            Assert.Equal(BiometricVerificationStatus.Approved, pending2.Status);
            mockRepository.Verify(x => x.UpdateAsync(It.IsAny<BiometricVerification>()), Times.Once);
        }
    }
}