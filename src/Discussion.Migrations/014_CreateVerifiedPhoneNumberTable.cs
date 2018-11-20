using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(14)]
    public class CreateVerifiedPhoneNumberTable : CreateEntityTableMigration
    {
        private const string TABLE_NAME = "VerifiedPhoneNumber";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("PhoneNumber").AsString();
        }
    }
}