using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Csharp.Api.Data;
using Csharp.Api.Services;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;
using Csharp.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace Csharp.Api.Tests.Unit.Services
{
    public class TagPositionProcessorTests : TestBase
    {
        private readonly Mock<ILogger<TagPositionProcessor>> _loggerMock;
        private readonly TagPositionProcessor _processor;

        // Entidades de teste
        private Pateo _pateo;
        private Zona _zonaVistoria, _zonaReparos;
        private Beacon _beaconVistoria, _beaconReparos;
        private TagBle _tag;
        private Moto _moto;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TagPositionProcessorTests()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _loggerMock = new Mock<ILogger<TagPositionProcessor>>();
            _processor = new TagPositionProcessor(_context, _loggerMock.Object);

            SetupDatabase();
        }

        private void SetupDatabase()
        {
            _pateo = new Pateo { Id = Guid.NewGuid(), Nome = "Pátio Teste", Status = "ATIVO", CreatedAt = DateTime.UtcNow };

            _zonaVistoria = new Zona { Id = Guid.NewGuid(), Nome = "ZONA DE VISTORIA", PateoId = _pateo.Id, CreatedAt = DateTime.UtcNow, CoordenadasWKT = "POLYGON(...)" };
            _zonaReparos = new Zona { Id = Guid.NewGuid(), Nome = "REPAROS LEVES", PateoId = _pateo.Id, CreatedAt = DateTime.UtcNow, CoordenadasWKT = "POLYGON(...)" };

            _beaconVistoria = new Beacon { Id = Guid.NewGuid(), BeaconId = "BEACON-VISTORIA", Ativo = true, ZonaId = _zonaVistoria.Id };
            _beaconReparos = new Beacon { Id = Guid.NewGuid(), BeaconId = "BEACON-REPAROS", Ativo = true, ZonaId = _zonaReparos.Id };

            _tag = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = "TAG-TESTE", NivelBateria = 90 };

            _moto = new Moto
            {
                Id = Guid.NewGuid(),
                Placa = "MOTO-TESTE",
                TagBleId = _tag.Id,
                Modelo = TipoModeloMoto.ModeloUrbana125,
                StatusMoto = TipoStatusMoto.AguardandoVistoria,
                ZonaId = _zonaVistoria.Id,
                DataCriacaoRegistro = DateTime.UtcNow
            };

            // histórico inicial
            var hist = new MotoZonaHistorico { Id = Guid.NewGuid(), MotoId = _moto.Id, ZonaId = _zonaVistoria.Id, EntradaEm = DateTime.UtcNow.AddMinutes(-10) };

            _context.Pateos.Add(_pateo);
            _context.Zonas.AddRange(_zonaVistoria, _zonaReparos);
            _context.Beacons.AddRange(_beaconVistoria, _beaconReparos);
            _context.TagsBle.Add(_tag);
            _context.Motos.Add(_moto);
            _context.MotoZonasHistorico.Add(hist);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ProcessAsync_ShouldUpdateBattery_WhenBatteryIsDifferent()
        {
            // Arrange
            var evento = new TagInteractionEventDto
            {
                CodigoUnicoTag = "TAG-TESTE",
                BeaconIdDetectado = "BEACON-VISTORIA",
                Timestamp = DateTime.UtcNow,
                NivelBateria = 80 // nivel de bateria mudou
            };

            // Act
            await _processor.ProcessAsync(evento);

            // Assert
            var tagNoDb = await _context.TagsBle.FindAsync(_tag.Id);
            Assert.Equal(80, tagNoDb?.NivelBateria);
        }

        [Fact]
        public async Task ProcessAsync_ShouldUpdateLocation_And_CreateNewHistory_WhenMotoMovesToNewZone()
        {
            // Arrange
            var timeMove = DateTime.UtcNow;
            var evento = new TagInteractionEventDto
            {
                CodigoUnicoTag = "TAG-TESTE",
                BeaconIdDetectado = "BEACON-REPAROS", // moto mudou de zona
                Timestamp = timeMove
            };

            // Act
            await _processor.ProcessAsync(evento);

            // Assert
            // moto atualizada
            var motoNoDb = await _context.Motos.FindAsync(_moto.Id);
            Assert.Equal(_beaconReparos.BeaconId, motoNoDb?.UltimoBeaconConhecidoId);
            Assert.Equal(_zonaReparos.Id, motoNoDb?.ZonaId); // Zona atualizada!

            // histórico atualizado
            var historicos = await _context.MotoZonasHistorico.OrderBy(h => h.EntradaEm).ToListAsync();
            Assert.Equal(2, historicos.Count);

            // histórico antigo (Vistoria) foi fechado
            Assert.Equal(_zonaVistoria.Id, historicos[0].ZonaId);
            Assert.Equal(timeMove, historicos[0].SaidaEm);

            // histórico novo (Reparos) foi aberto
            Assert.Equal(_zonaReparos.Id, historicos[1].ZonaId);
            Assert.Null(historicos[1].SaidaEm);
        }

        [Fact]
        public async Task ProcessAsync_ShouldOnlyUpdateTimestamp_WhenBeaconIsInSameZone()
        {
            // Arrange
            var timeMove = DateTime.UtcNow;
            var evento = new TagInteractionEventDto
            {
                CodigoUnicoTag = "TAG-TESTE",
                BeaconIdDetectado = "BEACON-VISTORIA", // <-- Mesmo beacon
                Timestamp = timeMove
            };

            // Act
            await _processor.ProcessAsync(evento);

            // Assert
            var motoNoDb = await _context.Motos.FindAsync(_moto.Id);
            Assert.Equal(timeMove, motoNoDb?.UltimaVezVistoEmPatio); // timestamp atualizado
            Assert.Equal(_zonaVistoria.Id, motoNoDb?.ZonaId);

            var historicoCount = await _context.MotoZonasHistorico.CountAsync();
            Assert.Equal(1, historicoCount); // nenhum histórico novo foi criado
        }
    }
}