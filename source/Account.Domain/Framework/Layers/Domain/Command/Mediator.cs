using System;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Domain.Command
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
                throw new InvalidOperationException($"Handler not found for {requestType.Name}.");

            var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResult>, TResult>.Handle));
            if (method == null)
                throw new InvalidOperationException($"Handle method not found for {requestType.Name}.");

            var task = (Task<TResult>)method.Invoke(handler, new object[] { request, cancellationToken });
            return await task;
        }
    }
}
