using AutoMapper;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Csharp.Api.Services
{
    public class PateoService : IPateoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PateoService> _logger;

        public PateoService(AppDbContext context, IMapper mapper, ILogger<PateoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PateoDetailDto> GetMyPateoAsync(string funcionarioTelefone)
        {
            _logger.LogInformation("Buscando pátio para o funcionário: {Telefone}", funcionarioTelefone);

            // 1. Acha o funcionário pelo telefone (que veio do JWT)
            var funcionario = await _context.Funcionarios
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Telefone == funcionarioTelefone);

            if (funcionario == null)
            {
                _logger.LogWarning("Funcionário com telefone {Telefone} não encontrado no banco de sync.", funcionarioTelefone);
                throw new RecursoNaoEncontradoException("Funcionário não encontrado.");
            }
            
            // 2. Acha o pátio (e suas zonas) usando o PateoId do funcionário
            var pateo = await _context.Pateos
                .AsNoTracking()
                .Include(p => p.Zonas) // Carrega as zonas
                .FirstOrDefaultAsync(p => p.Id == funcionario.PateoId);
                
            if (pateo == null)
            {
                _logger.LogError("Falha de integridade: Funcionário {FuncId} está associado ao PateoId {PateoId}, mas o pátio não foi encontrado no sync.", 
                    funcionario.Id, funcionario.PateoId);
                throw new RecursoNaoEncontradoException("Pátio associado não encontrado.");
            }

            // 3. Mapeia para a DTO de resposta
            return _mapper.Map<PateoDetailDto>(pateo);
        }
    }
}