using System.ComponentModel.DataAnnotations;
using Csharp.Api.DTOs;
using Csharp.Api.Entities.Enums;
using System.Diagnostics;

namespace Csharp.Api.DTOs.ValidationAttributes
{
    public class PlacaCondicionalObrigatoriaAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "A placa é obrigatória quando o estado da moto não é 'Sem Placa' durante a coleta.";

        public PlacaCondicionalObrigatoriaAttribute() : base(DefaultErrorMessage) { }
      
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
            {
                Debug.WriteLine("PlacaCondicionalObrigatoriaAttribute: validationContext.ObjectInstance é NULL.");
                return new ValidationResult("Contexto de validação não possui instância do objeto.");
            }

            Debug.WriteLine($"PlacaCondicionalObrigatoriaAttribute: Tipo de validationContext.ObjectInstance é {validationContext.ObjectInstance.GetType().FullName}");

            if (!(validationContext.ObjectInstance is CreateMotoDto dto))
            {
                Debug.WriteLine("PlacaCondicionalObrigatoriaAttribute: ObjectInstance NÃO É CreateMotoDto.");
                return new ValidationResult($"Atributo PlacaCondicionalObrigatoriaAttribute foi usado em um tipo inesperado: {validationContext.ObjectInstance.GetType().FullName}");
            }

            if (dto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(dto.Placa))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { nameof(dto.Placa), nameof(dto.StatusMoto) });
            }

            return ValidationResult.Success;
        }
    }
}
