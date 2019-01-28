using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(22)]
    public class AddUserIdToWeChatAccountTable : Migration
    {
        public override void Up()
        {
            Alter.Table(CreateWeChatAccountTable.TABLE_NAME)
                .AddColumn("UserId").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("UserId")
                .FromTable(CreateWeChatAccountTable.TABLE_NAME);
        }
    }
}
