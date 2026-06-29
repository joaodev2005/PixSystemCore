using System.Data;
using FluentMigrator;

namespace PixSystemCore.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersion.TABLE_ACCOUNTS, "Criação das tabelas Accounts e Transactions")]
public class Version0000001 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("Accounts")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("HolderName").AsString(200).NotNullable()
            .WithColumn("PixKey").AsString(100).NotNullable()
            .WithColumn("PixKeyType").AsInt32().NotNullable()
            .WithColumn("Balance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("BlockedBalance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.Index("IX_Accounts_PixKey")
            .OnTable("Accounts")
            .OnColumn("PixKey").Ascending()
            .WithOptions().Unique();

        Create.Table("Transactions")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("SourceAccountId").AsGuid().NotNullable()
            .WithColumn("TargetAccountId").AsGuid().NotNullable()
            .WithColumn("Amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("ProcessedAt").AsDateTime().Nullable()
            .WithColumn("ErrorMessage").AsString(500).Nullable()
            .WithColumn("IdempotencyKey").AsString(100).Nullable();

        Create.Index("IX_Transactions_IdempotencyKey")
            .OnTable("Transactions")
            .OnColumn("IdempotencyKey").Ascending();

        Create.ForeignKey("FK_Transactions_SourceAccount")
            .FromTable("Transactions").ForeignColumn("SourceAccountId")
            .ToTable("Accounts").PrimaryColumn("Id")
            .OnDelete(Rule.None);

        Create.ForeignKey("FK_Transactions_TargetAccount")
            .FromTable("Transactions").ForeignColumn("TargetAccountId")
            .ToTable("Accounts").PrimaryColumn("Id")
            .OnDelete(Rule.None);
    }
}
