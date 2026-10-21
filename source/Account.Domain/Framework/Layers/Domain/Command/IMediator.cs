using System.Threading;
using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Domain.Command
{
    public interface IMediator
    {
        Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
    }
}
