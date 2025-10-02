using System;

namespace Csharp.Api.Exceptions
{
    public class ConcorrenciaException : BusinessRuleException
    {
        public ConcorrenciaException(string message) 
            : base(message) { }
    }
}