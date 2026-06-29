using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PixSystemCore.Domain.Interfaces;
using PixSystemCore.Infrastructure;
using PixSystemCore.Infrastructure.Data;
using PixSystemCore.Infrastructure.Messaging;
using PixSystemCore.Infrastructure.Migrations;
using PixSystemCore.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<PixSystemCoreDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(DatabaseMigrations).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(PixSystemCore.Application.Commands.OpenAccountCommand).Assembly
    );
});

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await ExecuteMigrationsAsync();

app.Run();

async Task ExecuteMigrationsAsync()
{
    using var scope = app.Services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    string connectionString = configuration.GetConnectionString("DefaultConnection")!;

    var sqlBuilder = new SqlConnectionStringBuilder(connectionString);
    string targetDatabase = sqlBuilder.InitialCatalog;

    sqlBuilder.InitialCatalog = "master";
    string masterConnectionString = sqlBuilder.ConnectionString;

    using (var connection = new SqlConnection(masterConnectionString))
    {
        await connection.OpenAsync();

        var checkCommand = new SqlCommand(
            $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{targetDatabase}') CREATE DATABASE [{targetDatabase}];",
            connection);

        await checkCommand.ExecuteNonQueryAsync();
    }

    DatabaseMigrations.ExecuteMigrations(scope.ServiceProvider);
}