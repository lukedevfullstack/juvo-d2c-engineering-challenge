using Account.Data.Stores;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Queries.Biometrics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Data.Queries
{
    public class BiometricVerificationQuery : IBiometricVerificationQuery
    {
        private readonly BiometricVerificationStore _store;

        public BiometricVerificationQuery(BiometricVerificationStore store)
        {
            _store = store;
        }

        public Task<BiometricVerification> GetByReference(string reference)
        {
            return Task.FromResult(_store.GetByReference(reference));
        }

        public Task<IEnumerable<BiometricVerification>> GetAllPending()
        {
            var pending = _store.GetAll()
                .Where(v => v.Status == BiometricVerificationStatus.Pending)
                .ToList();
            
            return Task.FromResult(pending.AsEnumerable());
        }
    }
}
