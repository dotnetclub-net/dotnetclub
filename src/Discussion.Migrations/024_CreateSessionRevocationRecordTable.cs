using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(24)]
    public class CreateSessionRevocationRecordTable : CreateEntityTableMigration
    {
        protected override string TableName()
        {
            return "SessionRevocationRecord";
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("SessionId").AsString()
                .WithColumn("Reason").AsString();
        }
    }
}
