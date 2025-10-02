using System;

namespace Csharp.Api.Exceptions
{
    public class BeaconNotFoundException : BusinessRuleException
    {
        public BeaconNotFoundException(Guid beaconId) 
            : base($"O beacon com ID '{beaconId}' não foi encontrado.") { }
        
        public BeaconNotFoundException(string beaconId)
            : base($"O beacon com o ID '{beaconId}' não foi encontrado.") { }
    }
}