using AutoMapper;
using Csharp.Api.Data;
using Csharp.Api.Profiles;
using Microsoft.EntityFrameworkCore;
using System;

namespace Csharp.Api.Tests.Unit
{
    /// <summary>
    /// Classe base para testes de serviço que precisam de um DbContext em memória e AutoMapper.
    /// Ela usa IDisposable para garantir que o banco de dados seja limpo após cada teste.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper _mapper;

        public TestBase()
        {
            // Memory DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfiles());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        // Limpeza
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}