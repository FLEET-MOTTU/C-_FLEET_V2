using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Csharp.Api.Data;
using Csharp.Api.Services;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;
using Csharp.Api.DTOs;
using Csharp.Api.Exceptions;
using System.Linq;


namespace Csharp.Api.Tests.Unit.Services
{
    public class MotoServiceTests : TestBase
    {
        private readonly Mock<ILogger<MotoService>> _loggerMock;
        private readonly MotoService _motoService;

        public MotoServiceTests()
        {
            _loggerMock = new Mock<ILogger<MotoService>>();
            _motoService = new MotoService(_context, _loggerMock.Object, _mapper);
        }



        // Testes de Create
        [Fact]
        public async Task CreateMotoAsync_ShouldCreateMoto_WhenDataIsValid()
        {
            // Arrange
            var dto = new CreateMotoDto
            {
                Placa = "ABC1D23",
                Modelo = TipoModeloMoto.ModeloUrbana125,
                StatusMoto = TipoStatusMoto.PendenteColeta,
                CodigoUnicoTagParaNovaTag = "TAG-VALIDA-001"
            };

            // Act
            var result = await _motoService.CreateMotoAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ABC1D23", result.Placa);
            Assert.Equal("TAG-VALIDA-001", result.Tag?.CodigoUnicoTag);
            var motoNoDb = await _context.Motos.FindAsync(result.Id);
            Assert.NotNull(motoNoDb);
        }


        [Fact]
        public async Task CreateMotoAsync_ShouldThrow_WhenTagAlreadyExists()
        {
            // Arrange
            var tagExistente = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-EXISTENTE", NivelBateria = 100 };
            _context.TagsBle.Add(tagExistente);
            await _context.SaveChangesAsync();

            var dto = new CreateMotoDto
            {
                CodigoUnicoTagParaNovaTag = "TAG-EXISTENTE"
            };

            // Act / Assert
            Func<Task> act = () => _motoService.CreateMotoAsync(dto);
            await Assert.ThrowsAsync<TagJaExisteException>(act);
        }


        [Fact]
        public async Task CreateMotoAsync_ShouldThrow_WhenPlacaAlreadyExists()
        {
            // Arrange
            var tag1 = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-001" };
            var motoExistente = new Moto
            {
                Id = Guid.NewGuid(),
                Placa = "PLACA-EXISTENTE",
                Modelo = TipoModeloMoto.ModeloSport100,
                StatusMoto = TipoStatusMoto.Alugada,
                TagBleId = tag1.Id
            };
            _context.TagsBle.Add(tag1);
            _context.Motos.Add(motoExistente);
            await _context.SaveChangesAsync();

            var dto = new CreateMotoDto
            {
                Placa = "PLACA-EXISTENTE",
                CodigoUnicoTagParaNovaTag = "TAG-NOVA-002"
            };

            // Act / Assert
            Func<Task> act = () => _motoService.CreateMotoAsync(dto);
            await Assert.ThrowsAsync<PlacaJaExisteException>(act);
        }



        //Testes do Fluxo de Coleta
        [Fact]
        public async Task UpsertPorPlacaAsync_ShouldCreateNewMotoAndTag_WhenPlacaAndTagAreNew()
        {
            // Act
            var result = await _motoService.UpsertPorPlacaAsync(
                "PLACA-NOVA",
                TipoModeloMoto.ModeloUrbana125,
                TipoStatusMoto.PendenteColeta,
                "TAG-NOVA-1"
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PLACA-NOVA", result.Placa);
            Assert.Equal("TAG-NOVA-1", result.Tag?.CodigoUnicoTag);

            var motoCount = await _context.Motos.CountAsync();
            var tagCount = await _context.TagsBle.CountAsync();
            Assert.Equal(1, motoCount);
            Assert.Equal(1, tagCount);
        }


        [Fact]
        public async Task UpsertPorPlacaAsync_ShouldUpdateExistingMoto_WhenPlacaIsFound()
        {
            // Arrange
            var tag1 = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-EXISTENTE-1" };
            var moto1 = new Moto { Id = Guid.NewGuid(), Placa = "PLACA-EXISTENTE", Modelo = TipoModeloMoto.ModeloSport100, StatusMoto = TipoStatusMoto.Alugada, TagBleId = tag1.Id };
            _context.TagsBle.Add(tag1);
            _context.Motos.Add(moto1);
            await _context.SaveChangesAsync();

            // Act
            // Chamando Upsert para a mesma placa, mas com uma TAG NOVA e status novo
            var result = await _motoService.UpsertPorPlacaAsync(
                "PLACA-EXISTENTE", 
                TipoModeloMoto.ModeloTrilha150,
                TipoStatusMoto.EmReparosSimples,
                "TAG-NOVA-2"
            );

            // Assert
            var motoNoDb = await _context.Motos.FindAsync(moto1.Id);
            var tagNovaNoDb = await _context.TagsBle.FirstOrDefaultAsync(t => t.CodigoUnicoTag == "TAG-NOVA-2");

            Assert.Equal(moto1.Id, result.Id); // mesma moto
            Assert.Equal(TipoModeloMoto.ModeloTrilha150, motoNoDb?.Modelo);
            Assert.Equal(TipoStatusMoto.EmReparosSimples, motoNoDb?.StatusMoto); // status atualizado
            Assert.NotNull(tagNovaNoDb); // nova tag criada
            Assert.Equal(tagNovaNoDb?.Id, motoNoDb?.TagBleId); // moto associada a nova tag
        }
            

        [Fact]
        public async Task UpsertPorPlacaAsync_ShouldThrow_WhenTagIsAssignedToDifferentMoto()
        {
            // Arrange
            // Moto A / Tag A
            var tagA = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-A" };
            var motoA = new Moto { Id = Guid.NewGuid(), Placa = "MOTO-A", TagBleId = tagA.Id, Modelo = TipoModeloMoto.ModeloSport100, StatusMoto = TipoStatusMoto.Alugada };
            _context.TagsBle.Add(tagA);
            _context.Motos.Add(motoA);
            
            // Moto B / Tag B
            var tagB = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-B" };
            var motoB = new Moto { Id = Guid.NewGuid(), Placa = "MOTO-B", TagBleId = tagB.Id, Modelo = TipoModeloMoto.ModeloSport100, StatusMoto = TipoStatusMoto.Alugada };
            _context.TagsBle.Add(tagB);
            _context.Motos.Add(motoB);
            await _context.SaveChangesAsync();

            // Act
            // Tenta fazer Upsert da Moto A, passando Tag B
            Func<Task> act = () => _motoService.UpsertPorPlacaAsync(
                "MOTO-A", 
                TipoModeloMoto.ModeloUrbana125, 
                TipoStatusMoto.PendenteColeta, 
                "TAG-B"
            );

            // Assert
            await Assert.ThrowsAsync<ConcorrenciaException>(act);
        }
    }
}