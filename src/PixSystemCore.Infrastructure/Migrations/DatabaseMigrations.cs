using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace PixSystemCore.Infrastructure.Migrations;

public static class DatabaseMigrations
{
    public static void ExecuteMigrations(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        runner.ListMigrations();

        runner.MigrateUp();
    }
}
