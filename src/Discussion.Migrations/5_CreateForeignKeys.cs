using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(5)]
    public class CreateForeignKeys: Migration
    {
        private string FK_NAME_TOPIC_USER = "fk_topic_user";
        private string FK_NAME_REPLY_TOPIC = "fk_reply_topic";
        private string FK_NAME_REPLY_USER = "fk_reply_topic";

        public override void Up()
        {
            Create.ForeignKey(FK_NAME_TOPIC_USER)
                .FromTable(CreateTopicTable.TABLE_NAME).ForeignColumn("CreatedBy")
                .ToTable(CreateUserTable.TABLE_NAME).PrimaryColumn("Id");

            Create.ForeignKey(FK_NAME_REPLY_TOPIC)
                .FromTable(CreateReplyTable.TABLE_NAME).ForeignColumn("TopicId")
                .ToTable(CreateTopicTable.TABLE_NAME).PrimaryColumn("Id");
            
            Create.ForeignKey(FK_NAME_REPLY_USER)
                .FromTable(CreateReplyTable.TABLE_NAME).ForeignColumn("CreatedBy")
                .ToTable(CreateUserTable.TABLE_NAME).PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey(FK_NAME_TOPIC_USER);
            Delete.ForeignKey(FK_NAME_REPLY_TOPIC);
            Delete.ForeignKey(FK_NAME_REPLY_USER);
        }
    }
}