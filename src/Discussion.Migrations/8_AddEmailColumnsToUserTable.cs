using FluentMigrator;

namespace Discussion.Migrations
{
    public class AddEmailColumnsToUserTable: Migration
    {
        public override void Up()
        {
            Alter.Table(CreateUserTable.TABLE_NAME)
                .AddColumn("EmailAddress").AsString(50).Nullable()
                .AddColumn("IsActivation").AsBoolean();
        }

        public override void Down()
        {
            Delete.Column("EmailAddress")
                .Column("IsActivation")
                .FromTable(CreateUserTable.TABLE_NAME);
        }
    }
}