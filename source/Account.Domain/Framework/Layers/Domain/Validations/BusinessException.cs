using System;

namespace Account.Domain.Framework.Layers.Domain.Validations
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}
