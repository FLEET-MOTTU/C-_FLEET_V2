using System.ComponentModel.DataAnnotations;
using Csharp.Api.DTOs;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.DTOs.ValidationAttributes
{
    public class PlacaCondicionalObrigatoriaAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "A placa é obrigatória quando o estado da moto não é 'Sem Placa' durante a coleta.";

        public PlacaCondicionalObrigatoriaAttribute() : base(DefaultErrorMessage) { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dto = (CreateMotoDto)validationaContext.ObjectInstance;

            if (dto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(dto.Placa))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { nameof(dto.Placa), nameof(dto.StatusMoto) });
            }

            return ValidationResult.Success;
        }
    }
}
