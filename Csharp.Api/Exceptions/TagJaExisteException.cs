using System;

namespace Csharp.Api.Exceptions
{
    public class TagJaExisteException : BusinessRuleException
    {
        public TagJaExisteException(string codigoUnicoTag) 
            : base($"A tag com código '{codigoUnicoTag}' já está registrada.") 
        {
        }

        public TagJaExisteException(string message, Exception innerException) 
            : base(message, innerException) 
        {
        }
    }
}
