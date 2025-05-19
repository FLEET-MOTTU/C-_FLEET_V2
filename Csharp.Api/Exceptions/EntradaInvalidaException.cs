using System;

namespace Csharp.Api.Exceptions
{
    public class EntradaInvalidaException : BusinessRuleException
    {
        public EntradaInvalidaException(string message) 
            : base(message) 
        {
        }

        public EntradaInvalidaException(string parameterName, string message) 
            : base($"Parâmetro de entrada inválido '{parameterName}': {message}") 
        {
        }

        public EntradaInvalidaException(string message, Exception innerException) 
            : base(message, innerException) 
        {
        }
    }
}
