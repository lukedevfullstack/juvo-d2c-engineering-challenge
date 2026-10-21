using Account.Domain.Framework.Layers.Data.UnitOfWork;
using System.Threading.Tasks;

namespace Account.Data.UnitOfWork
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private bool _hasTransaction;

        public void BeginTransaction()
        {
            _hasTransaction = true;
        }

        public Task CommitAsync()
        {
            if (_hasTransaction)
                _hasTransaction = false;

            return Task.CompletedTask;
        }

        public void Rollback()
        {
            if (_hasTransaction)
                _hasTransaction = false;
        }
    }
}
