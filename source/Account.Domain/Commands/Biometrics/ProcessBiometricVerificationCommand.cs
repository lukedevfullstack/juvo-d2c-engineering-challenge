using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Account.Domain.Aggregates.Biometrics;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Responses;

namespace Account.Domain.Commands.Biometrics
{
    public class ProcessBiometricVerificationCommand : IRequest<BiometricVerificationResponse>
    {
        public ProcessBiometricVerificationRequest Request { get; }

        public ProcessBiometricVerificationCommand(ProcessBiometricVerificationRequest request)
        {
            Request = request;
        }

        public class ProcessBiometricVerificationRequest
        {
            [Required(ErrorMessage = "Campo obrigatório")]
            public string Reference { get; set; }
        }
    }

    public class ProcessBiometricVerificationCommandHandler : CommandHandlerBase<BiometricVerification, ProcessBiometricVerificationCommand, BiometricVerificationResponse, BiometricVerificationAggregate>
    {
        private readonly IBiometricVerificationQuery _query;
        private readonly IRepository _repository;
        private readonly IBiometricProvider _provider;

        public ProcessBiometricVerificationCommandHandler(
            IServiceProvider serviceProvider,
            IBiometricVerificationQuery query,
            IRepository repository,
            IBiometricProvider provider) : base(serviceProvider)
        {
            _query = query;
            _repository = repository;
            _provider = provider;
        }

        protected override async Task<CommandHandlerResponse<BiometricVerification>> HandleRequest(ProcessBiometricVerificationCommand command, BiometricVerificationAggregate aggregate, CancellationToken cancellationToken)
        {
            if (command?.Request == null || string.IsNullOrWhiteSpace(command.Request.Reference))
                throw new BusinessException("Referência da verificação é obrigatória.");

            var reference = command.Request.Reference;
            var current = await _query.GetByReference(reference);

            // Se já existe verificação, retorna o estado atual
            if (current != null)
                return CommandHandlerResponse<BiometricVerification>.UpdateResult(current.Id, current);

            // Consulta o provedor para obter o status
            var providerResponse = await _provider.GetByReference(reference);
            
            // Converte a resposta do provedor para o modelo de domínio
            var status = ConvertProviderStatusToDomainStatus(providerResponse.Status);
            
            // Cria nova verificação
            var verification = BiometricVerification.New();
            verification.Reference = reference;
            verification.Status = status;
            verification.ResultDescription = GetResultDescription(status);

            // Persiste usando o aggregate
            aggregate.ApplyResult(verification, status, verification.ResultDescription);

            // Salva no repositório
            await _repository.AddAsync(verification);

            return CommandHandlerResponse<BiometricVerification>.CreateResult(verification.Id, verification);
        }

        protected override BiometricVerificationResponse Map(BiometricVerification entity)
        {
            return BiometricVerificationResponse.From(entity);
        }

        private BiometricVerificationStatus ConvertProviderStatusToDomainStatus(string providerStatus)
        {
            return providerStatus switch
            {
                "APPROVED" => BiometricVerificationStatus.Approved,
                "REJECTED" => BiometricVerificationStatus.Rejected,
                "PENDING" => BiometricVerificationStatus.Pending,
                _ => BiometricVerificationStatus.Pending
            };
        }

        private string GetResultDescription(BiometricVerificationStatus status)
        {
            return status switch
            {
                BiometricVerificationStatus.Approved => "Verificação de biometria aprovada",
                BiometricVerificationStatus.Rejected => "Verificação de biometria reprovada",
                BiometricVerificationStatus.Pending => "Verificação de biometria em processamento",
                _ => null
            };
        }
    }
}
