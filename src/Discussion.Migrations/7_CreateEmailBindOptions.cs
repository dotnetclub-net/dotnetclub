using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(7)]
    public class CreateEmailBindOptionsTable: CreateEntityTableMigration
    {
        public const string TABLE_NAME = "EmailBindOptions";

        protected override string TableName()
        {
            return TABLE_NAME;
        }
        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable
                .WithColumn("UserId").AsInt32()
                .WithColumn("EmailAddress").AsString(50)
                .WithColumn("OldEmailAddress").AsString(50)
                .WithColumn("CallbackToken").AsString(300)
                .WithColumn("IsActivated").AsBoolean();
        }

    }
}