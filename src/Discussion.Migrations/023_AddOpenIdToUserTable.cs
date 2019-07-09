using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(23)]
    public class AddOpenIdToUserTable : Migration
    {
        public override void Up()
        {
            Alter.Table(CreateUserTable.TABLE_NAME)
                .AddColumn("OpenIdProvider").AsString().Nullable()
                .AddColumn("OpenId").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("OpenIdProvider")
                .FromTable(CreateUserTable.TABLE_NAME);
            Delete.Column("OpenId")
                .FromTable(CreateUserTable.TABLE_NAME);
        }
    }
}
