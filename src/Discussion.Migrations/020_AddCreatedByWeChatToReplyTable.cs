using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(20)]
    public class AddCreatedByWeChatToReplyTable : Migration
    {
        public override void Up()
        {
            Alter.Table(CreateReplyTable.TABLE_NAME)
                .AddColumn("CreatedByWeChat").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("CreatedByWeChat").FromTable(CreateReplyTable.TABLE_NAME);
        }
    }
}
