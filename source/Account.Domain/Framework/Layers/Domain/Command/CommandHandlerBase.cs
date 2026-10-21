using Account.Domain.Framework;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Account.Domain.Framework.Layers.Domain.Entity;
using Account.Domain.Framework.Layers.Domain.Validations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Domain.Framework.Layers.Domain.Command
{
    public abstract class CommandHandlerBase<TEntity, TRequest, TResult, TAggregate> : IRequestHandler<TRequest, TResult>
        where TEntity : Entity<TEntity>
        where TRequest : IRequest<TResult>
        where TAggregate : class, new()
    {
        private readonly IUnitOfWork _unitOfWork;

        protected CommandHandlerBase(IServiceProvider serviceProvider)
        {
            _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        }

        public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var aggregate = new TAggregate();

            try
            {
                _unitOfWork.BeginTransaction();

                var result = await HandleRequest(request, aggregate, cancellationToken);
                var typedResult = Map(result.Data);

                await _unitOfWork.CommitAsync();
                return typedResult;
            }
            catch (BusinessException)
            {
                _unitOfWork.Rollback();
                throw;
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        protected abstract Task<CommandHandlerResponse<TEntity>> HandleRequest(TRequest request, TAggregate aggregate, CancellationToken cancellationToken);

        protected abstract TResult Map(TEntity entity);
    }
}
