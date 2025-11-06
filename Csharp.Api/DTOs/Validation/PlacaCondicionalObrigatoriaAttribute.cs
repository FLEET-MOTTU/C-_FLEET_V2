using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.DTOs.Validation
{
    /// <summary>Exige placa quando o status NÃO é "SemPlacaEmColeta".</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PlacaCondicionalObrigatoriaAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage =
            "A placa é obrigatória quando o estado não é 'SemPlacaEmColeta'.";

        public PlacaCondicionalObrigatoriaAttribute() : base(DefaultErrorMessage) { }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not CreateMotoDto dto)
            {
                Debug.WriteLine("PlacaCondicionalObrigatoriaAttribute: tipo inesperado.");
                return ValidationResult.Success; // não bloqueia outros DTOs
            }

            if (dto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(dto.Placa))
            {
                return new ValidationResult(
                    ErrorMessage,
                    new[] { nameof(CreateMotoDto.Placa), nameof(CreateMotoDto.StatusMoto) }
                );
            }

            return ValidationResult.Success;
        }
    }
}
