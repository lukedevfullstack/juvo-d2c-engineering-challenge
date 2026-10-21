using Account.Domain.Commands.Biometrics;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Responses;
using Account.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using static Account.Domain.Commands.Biometrics.ProcessBiometricVerificationCommand;

namespace Account.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBiometricVerificationQuery _query;
        private readonly ILoanResources _loanResources;

        public AccountController(
            IMediator mediator,
            IBiometricVerificationQuery query,
            ILoanResources loanResources)
        {
            _mediator = mediator;
            _query = query;
            _loanResources = loanResources;
        }

        [HttpPost]
        [Route("biometrics/verifications")]
        [ProducesResponseType(typeof(BiometricVerificationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ProcessBiometricVerification(ProcessBiometricVerificationRequest request)
        {
            var command = new ProcessBiometricVerificationCommand(request);
            var response = await _mediator.Send(command);

            await _loanResources.UpdateRegistrationStep(request.Reference, "FacialBiometry");

            return Ok(response);
        }

        [HttpGet]
        [Route("biometrics/verifications/{reference}")]
        [ProducesResponseType(typeof(BiometricVerificationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetBiometricVerification(string reference)
        {
            var verification = await _query.GetByReference(reference);
            if (verification == null)
                return NotFound();

            return Ok(BiometricVerificationResponse.From(verification));
        }

        [HttpGet]
        [Route("loans/registration-steps/{reference}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLoanRegistrationStep(string reference)
        {
            var step = await _loanResources.GetRegistrationStep(reference);
            if (string.IsNullOrWhiteSpace(step))
                return NotFound();

            return Ok(new { reference, step });
        }
    }
}
