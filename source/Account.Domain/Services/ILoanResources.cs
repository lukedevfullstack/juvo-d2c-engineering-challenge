using System.Threading.Tasks;

namespace Account.Domain.Services
{
    public interface ILoanResources
    {
        Task UpdateRegistrationStep(string reference, string step);
        Task<string> GetRegistrationStep(string reference);
    }
}
