using System.Threading;
using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Domain.Command
{
    public interface IRequestHandler<TRequest, TResult> where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
