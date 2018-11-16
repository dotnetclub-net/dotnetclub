using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(13)]
    public class AddLastRepliedByToTopicTable: Migration
    {
        public override void Up()
        {
            Alter.Table(CreateTopicTable.TABLE_NAME)
                .AddColumn("LastRepliedBy")
                .AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("LastRepliedBy").FromTable(CreateTopicTable.TABLE_NAME);
        }
    }
}