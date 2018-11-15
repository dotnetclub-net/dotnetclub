using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(8)]
    public class AddEmailColumnsToUserTable : Migration
    {
        public override void Up()
        {
            Alter.Table(CreateUserTable.TABLE_NAME)
                .AddColumn("EmailAddress").AsString(50).Nullable()
                .AddColumn("EmailAddressConfirmed").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("EmailAddress").FromTable(CreateUserTable.TABLE_NAME);
            Delete.Column("EmailAddressConfirmed").FromTable(CreateUserTable.TABLE_NAME);
        }
    }
}