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

builder.Services.AddOpenApi(options =>{});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
