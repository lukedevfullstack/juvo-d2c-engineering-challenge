using Account.Domain.Commands.Biometrics;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Account.Domain.Commands.Biometrics.ProcessBiometricVerificationCommand;

namespace Account.Tests
{
    public class ProcessBiometricVerificationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBiometricVerificationQuery> _mockQuery;
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<IBiometricProvider> _mockProvider;
        private readonly IServiceProvider _serviceProvider;

        public ProcessBiometricVerificationCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockQuery = new Mock<IBiometricVerificationQuery>();
            _mockRepository = new Mock<IRepository>();
            _mockProvider = new Mock<IBiometricProvider>();

            // Configurar IServiceProvider para retornar mock do IUnitOfWork
            var services = new Mock<IServiceProvider>();
            services
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            _serviceProvider = services.Object;
        }

        [Fact]
        public async Task Handle_WithApprovedReference_ShouldCreateVerificationWithApprovedStatus()
        {
            // Arrange
            var reference = "verification-approved-1";
            var request = new ProcessBiometricVerificationRequest { Reference = reference };
            var command = new ProcessBiometricVerificationCommand(request);

            _mockQuery
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync((BiometricVerification)null);

            _mockProvider
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync(new BiometricProviderResponse { Status = "APPROVED" });

            _mockRepository
                .Setup(x => x.AddAsync(It.IsAny<BiometricVerification>()))
                .Returns(Task.CompletedTask);

            var handler = new ProcessBiometricVerificationCommandHandler(
                _serviceProvider,
                _mockQuery.Object,
                _mockRepository.Object,
                _mockProvider.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Approved", result.Status);
            Assert.Equal("Verificação de biometria aprovada", result.ResultDescription);
            Assert.Equal(reference, result.Reference);
            _mockRepository.Verify(x => x.AddAsync(It.IsAny<BiometricVerification>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithRejectedReference_ShouldCreateVerificationWithRejectedStatus()
        {
            // Arrange
            var reference = "verification-rejected-1";
            var request = new ProcessBiometricVerificationRequest { Reference = reference };
            var command = new ProcessBiometricVerificationCommand(request);

            _mockQuery
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync((BiometricVerification)null);

            _mockProvider
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync(new BiometricProviderResponse { Status = "REJECTED" });

            _mockRepository
                .Setup(x => x.AddAsync(It.IsAny<BiometricVerification>()))
                .Returns(Task.CompletedTask);

            var handler = new ProcessBiometricVerificationCommandHandler(
                _serviceProvider,
                _mockQuery.Object,
                _mockRepository.Object,
                _mockProvider.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Rejected", result.Status);
            Assert.Equal("Verificação de biometria reprovada", result.ResultDescription);
        }

        [Fact]
        public async Task Handle_WithPendingReference_ShouldCreateVerificationWithPendingStatus()
        {
            // Arrange
            var reference = "verification-pending-1";
            var request = new ProcessBiometricVerificationRequest { Reference = reference };
            var command = new ProcessBiometricVerificationCommand(request);

            _mockQuery
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync((BiometricVerification)null);

            _mockProvider
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync(new BiometricProviderResponse { Status = "PENDING" });

            _mockRepository
                .Setup(x => x.AddAsync(It.IsAny<BiometricVerification>()))
                .Returns(Task.CompletedTask);

            var handler = new ProcessBiometricVerificationCommandHandler(
                _serviceProvider,
                _mockQuery.Object,
                _mockRepository.Object,
                _mockProvider.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending", result.Status);
            Assert.Equal("Verificação de biometria em processamento", result.ResultDescription);
        }

        [Fact]
        public async Task Handle_WithExistingReference_ShouldReturnCurrentState()
        {
            // Arrange
            var reference = "verification-existing-1";
            var request = new ProcessBiometricVerificationRequest { Reference = reference };
            var command = new ProcessBiometricVerificationCommand(request);

            var existingVerification = BiometricVerification.New();
            existingVerification.Reference = reference;
            existingVerification.Status = BiometricVerificationStatus.Approved;
            existingVerification.ResultDescription = "Verificação de biometria aprovada";

            _mockQuery
                .Setup(x => x.GetByReference(reference))
                .ReturnsAsync(existingVerification);

            var handler = new ProcessBiometricVerificationCommandHandler(
                _serviceProvider,
                _mockQuery.Object,
                _mockRepository.Object,
                _mockProvider.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Approved", result.Status);
            Assert.Equal(reference, result.Reference);
            _mockProvider.Verify(x => x.GetByReference(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(x => x.AddAsync(It.IsAny<BiometricVerification>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithEmptyReference_ShouldThrowBusinessException()
        {
            // Arrange
            var request = new ProcessBiometricVerificationRequest { Reference = "" };
            var command = new ProcessBiometricVerificationCommand(request);

            var handler = new ProcessBiometricVerificationCommandHandler(
                _serviceProvider,
                _mockQuery.Object,
                _mockRepository.Object,
                _mockProvider.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => handler.Handle(command, CancellationToken.None));
        }
    }
}