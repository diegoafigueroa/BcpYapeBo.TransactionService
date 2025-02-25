using BcpYapeBo.Transaction.Infrastructure.Repositories;
using BcpYapeBo.Transaction.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.API.Common;
using BcpYapeBo.Transaction.Infrastructure.Messaging;
using BcpYapeBo.Transaction.Application.Ports.Driving;
using BcpYapeBo.Transaction.Application.Ports.Driven;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console();
    config.ReadFrom.Configuration(context.Configuration);
});

// Configurar PostgreSQL con la conexi�n del appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TransactionDbContext>(options => options.UseNpgsql(connectionString));

// ADD SERVICES TO THE CONTAINER
builder.Services.AddControllers();

// REGISTER INTERNAL/EXTERNAL PORTS IN THE DEPENDENCY CONTAINER (WITH THEIR ADAPTERS)
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<ITransactionAntiFraudService, TransactionAntiFraudServiceKafka>();

// TODO
builder.Services.AddHostedService<TransactionAntiFraudStatusConsumerKafka>();

// ADD SWAGGER CONFIGURATION
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // VISUALIZE XML COMMENTS FROM CODE
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BcpYapeBo.Transaction.API.xml"));

    // SWAGGER BRANDING
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API. Yape Bolivia Transacciones",
        Version = "v1",
        Description = "API que maneja las transacciones de Yape Bolivia"
    });
});

// BUILD THE WEB APPLICATION.
var app = builder.Build();

// CONFIGURE THE HTTP REQUEST PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Activates Swagger generation
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Yape API v1");  // Swagger JSON endpoint
        c.RoutePrefix = string.Empty;  // Serve Swagger UI at the root of the server
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

