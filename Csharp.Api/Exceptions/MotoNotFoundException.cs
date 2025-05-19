using System;

namespace Csharp.Api.Exceptions
{
    public class MotoNotFoundException : BusinessRuleException 
    {
        public MotoNotFoundException(Guid motoId) 
            : base($"A moto com ID '{motoId}' não foi encontrada.") 
        {
        }

        public MotoNotFoundException(string placa) 
            : base($"A moto com placa '{placa}' não foi encontrada.") 
        {
        }

        public MotoNotFoundException(string message) 
            : base(message) 
        {
        }
    }
}
