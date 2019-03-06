using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(21)]
    public class AddLastRepliedByWeChatToTopicTable : Migration
    {
        public override void Up()
        {
            Alter.Table(CreateTopicTable.TABLE_NAME)
                .AddColumn("LastRepliedByWeChat").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("LastRepliedByWeChat")
                .FromTable(CreateTopicTable.TABLE_NAME);
        }
    }
}
