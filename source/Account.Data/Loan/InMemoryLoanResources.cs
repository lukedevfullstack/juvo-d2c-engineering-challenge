using Account.Domain.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Account.Data.Loan
{
    public class InMemoryLoanResources : ILoanResources
    {
        private readonly ConcurrentDictionary<string, string> _steps = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Task UpdateRegistrationStep(string reference, string step)
        {
            _steps[reference] = step;
            return Task.CompletedTask;
        }

        public Task<string> GetRegistrationStep(string reference)
        {
            _steps.TryGetValue(reference, out var step);
            return Task.FromResult(step);
        }
    }
}
