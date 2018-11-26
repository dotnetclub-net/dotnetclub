using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(15)]
    public class AddPhoneNumberIdToUserTable: Migration
    {
        public override void Up()
        {
            Alter.Table(CreateUserTable.TABLE_NAME)
                .AddColumn("PhoneNumberId").AsInt32()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("PhoneNumberId").FromTable(CreateUserTable.TABLE_NAME);
        }
    }
}