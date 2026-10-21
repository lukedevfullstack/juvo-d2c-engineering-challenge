using Account.Data.Stores;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Domain.Entity;
using System;
using System.Threading.Tasks;

namespace Account.Data.Repositories
{
    public class Repository : IRepository
    {
        private readonly BiometricVerificationStore _store;

        public Repository(BiometricVerificationStore store)
        {
            _store = store;
        }

        public Task AddAsync<TEntity>(TEntity entity) where TEntity : Entity<TEntity>
        {
            if (entity is BiometricVerification verification)
            {
                _store.Add(verification);
                return Task.CompletedTask;
            }

            throw new NotSupportedException($"Repository does not support {typeof(TEntity).Name}.");
        }

        public Task UpdateAsync<TEntity>(TEntity entity) where TEntity : Entity<TEntity>
        {
            if (entity is BiometricVerification verification)
            {
                _store.Update(verification);
                return Task.CompletedTask;
            }

            throw new NotSupportedException($"Repository does not support {typeof(TEntity).Name}.");
        }
    }
}
