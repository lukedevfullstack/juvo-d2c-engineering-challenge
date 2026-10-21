using Account.Domain.Framework.Layers.Domain.Entity;

namespace Account.Domain.Framework.Layers.Domain.Command
{
    public class CommandHandlerResponse<T> where T : class
    {
        public EntityId AffectedEntityId { get; }
        public T Data { get; }

        private CommandHandlerResponse(EntityId affectedEntityId, T data)
        {
            AffectedEntityId = affectedEntityId;
            Data = data;
        }

        public static CommandHandlerResponse<T> CreateResult(EntityId affectedEntityId, T data)
        {
            return new CommandHandlerResponse<T>(affectedEntityId, data);
        }

        public static CommandHandlerResponse<T> UpdateResult(EntityId affectedEntityId, T data)
        {
            return new CommandHandlerResponse<T>(affectedEntityId, data);
        }
    }
}
