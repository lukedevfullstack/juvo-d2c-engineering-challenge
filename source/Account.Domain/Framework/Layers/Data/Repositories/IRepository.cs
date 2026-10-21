using Account.Domain.Framework.Layers.Domain.Entity;
using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Data.Repositories
{
    public interface IRepository
    {
        Task AddAsync<TEntity>(TEntity entity) where TEntity : Entity<TEntity>;
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : Entity<TEntity>;
    }
}
