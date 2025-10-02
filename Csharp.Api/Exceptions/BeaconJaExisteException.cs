using System;

namespace Csharp.Api.Exceptions
{
    public class BeaconJaExisteException : BusinessRuleException
    {
        public BeaconJaExisteException(string beaconId) 
            : base($"O beacon com ID '{beaconId}' já está registrado.") { }
    }
}