using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        void BeginTransaction();
        Task CommitAsync();
        void Rollback();
    }
}
