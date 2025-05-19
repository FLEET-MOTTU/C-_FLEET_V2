using System;

namespace Csharp.Api.Exceptions
{
    public class PlacaJaExisteException : BusinessRuleException 
    {
        public PlacaJaExisteException(string placa) 
            : base($"A moto com placa '{placa}' já está registrada.") 
        {
        }

        public PlacaJaExisteException(string message, Exception innerException) 
            : base(message, innerException) 
        {
        }
    }
}
