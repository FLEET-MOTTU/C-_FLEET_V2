using System;

namespace Csharp.Api.Exceptions
{
    /// <summary>
    /// Exceção genérica para erros 404 (Not Found).
    /// Disparada quando um recurso (Pátio, Funcionário, etc.) não é encontrado no banco.
    /// </summary>
    public class RecursoNaoEncontradoException : Exception
    {
        public RecursoNaoEncontradoException(string message) 
            : base(message)
        {
        }

        public RecursoNaoEncontradoException(string resourceName, object resourceId)
            : base($"O recurso '{resourceName}' com ID '{resourceId}' não foi encontrado.")
        {
        }
    }
}