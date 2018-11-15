using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(3)]
    public class CreateTopicTable: CreateEntityTableMigration
    {
        public const string TABLE_NAME = "Topic";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("Title").AsString(255).NotNullable()
                .WithColumn("Content").AsString(int.MaxValue).Nullable()
                .WithColumn("Type").AsInt16().NotNullable()
                .WithColumn("CreatedBy").AsInt32().NotNullable()
                .WithColumn("LastRepliedAt").AsDateTime().Nullable()
                .WithColumn("ReplyCount").AsInt32()
                .WithColumn("ViewCount").AsInt32();
        }
    }
}