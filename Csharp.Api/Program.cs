using Microsoft.EntityFrameworkCore;
using Csharp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnection");

if (string.IsNullOrEmpty(oracleConnectionString))
{
    throw new InvalidOperationException("OracleConnection não encontrada");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt =>{})
);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("INFO: Verificando e aplicando migrations pendentes do EF Core...");
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();            
            dbContext.Database.Migrate();            
            Console.WriteLine("INFO: Migrations aplicadas com sucesso (ou nenhuma estava pendente).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO: Ocorreu um erro ao aplicar as migrations do EF Core: {ex.Message}");
        Console.WriteLine($"Detalhes: {ex.StackTrace}");        
        throw;
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// app.UseRouting();
// app.UseAuthorization();
app.MapControllers();
app.Run();
