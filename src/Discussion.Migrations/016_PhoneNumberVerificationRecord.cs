using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(16)]
    public class CreatePhoneNumberVerificationRecordTable : CreateEntityTableMigration
    {
        private const string TABLE_NAME = "PhoneNumberVerificationRecord";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("UserId").AsInt32()
                .WithColumn("PhoneNumber").AsString()
                .WithColumn("Code").AsString()
                .WithColumn("Expires").AsDateTime();
        }
    }
}